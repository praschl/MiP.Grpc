using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Proto = Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using MiP.Grpc.Internal;

namespace MiP.Grpc
{
    internal class DispatcherCompiler
    {
        private const string Base = "Base";
        private const string Dispatcher = "Dispatcher";
        private const string GlobalPrefix = "global::";

        private static class Tag
        {
            public const string Class = "{Class}";
            public const string BaseClass = "{BaseClass}";
            public const string Members = "{Members}";

            public const string Constructor = "{Constructor}";
            public const string Method = "{Method}";
            public const string Request = "{Request}";
            public const string Response = "{Response}";
            public const string Handler = "{Handler}";

            public const string Policy = "{Policy}";
            public const string Roles = "{Roles}";
            public const string AuthenticationSchemes = "{AuthenticationSchemes}";
            public const string Attributes = "{Attributes}";
            public const string AttributeProperties = "{AttributeProperties}";
        }

        private static class Code
        {
            public const string ClassCode = @"{Attributes}
public class {Class} : {BaseClass}
{{Members}}
";

            public const string ConstructorCode = @"
    private readonly global::MiP.Grpc.IDispatcher _dispatcher;
    public {Constructor}(global::MiP.Grpc.IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
";

            public const string MethodHandlerCode = @"{Attributes}
    public async override global::System.Threading.Tasks.Task<{Response}> {Method}({Request} request, global::Grpc.Core.ServerCallContext context)
    {
        return await _dispatcher.Dispatch<{Request}, {Response}, {Handler}>(request, context);
    }
";

            public const string MethodCommandHandlerCode = @"{Attributes}
    public async override global::System.Threading.Tasks.Task<global::Google.Protobuf.WellKnownTypes.Empty> {Method}({Request} request, global::Grpc.Core.ServerCallContext context)
    {
        return await _dispatcher.Dispatch<{Request}, global::Google.Protobuf.WellKnownTypes.Empty, global::MiP.Grpc.ICommandHandlerAdapter<{Request}, {Handler}>>(request, context);
    }
";

            public const string ReturnTypeCode = @"
return typeof({Class});
";

            public const string AuthorizeAttributeCode = @"
    [global::Microsoft.AspNetCore.Authorization.Authorize({AttributeProperties})]";

            public const string PolicyPropertyCode = @"Policy = ""{Policy}""";
            public const string RolesPropertyCode = @"Roles = ""{Roles}""";
            public const string AuthenticationSchemesPropertyCode = @"AuthenticationSchemes = ""{AuthenticationSchemes}""";
        }

        private readonly IHandlerStore _handlerStore;
        private readonly MapGrpcServiceConfiguration _configuration;

        public DispatcherCompiler(IHandlerStore handlerStore, MapGrpcServiceConfiguration configuration)
        {
            _handlerStore = handlerStore;
            _configuration = configuration;
        }

        public Type CompileDispatcher(Type serviceBaseType)
        {
            try
            {
                var result = GenerateSource(serviceBaseType);

                var dispatcherType = CompileToType(result.Source, result.UsedTypes);

                return dispatcherType;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Generating implementation for service [{serviceBaseType.FullName}] failed", ex);
            }
        }

        private static Type CompileToType(string source, IEnumerable<Type> importTypes)
        {
            try
            {
                var types = new[]
                {
                    typeof(Task<>),
                    typeof(IDispatcher),
                    typeof(ICommandHandlerAdapter<,>),
                    typeof(ServerCallContext),
                    typeof(AuthorizeAttribute),
                    typeof(Proto.Empty)
                }.Concat(importTypes);

                var compiledType = CSharpScript.EvaluateAsync<Type>(source,
                    ScriptOptions.Default
                        .WithReferences(
                            types.Select(t => t.Assembly).Distinct()
                            )
                    )
                    .GetAwaiter().GetResult();

                return compiledType;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Compilation failed for source: "
                    + Environment.NewLine
                    + "---"
                    + source
                    + "---"
                    , ex);
            }
        }

        private GenerateSourceResult GenerateSource(Type serviceBaseType)
        {
            var handlerMaps = GetMethodsToImplement(serviceBaseType);

            var className = GetGeneratedName(serviceBaseType);

            var baseClassName = GetFullClassName(serviceBaseType); // + is used by framework for nested classes

            // get source of the class with all its methods
            var source = GenerateSource(handlerMaps, className, baseClassName);

            // add a line which returns the Type of that class from the script.
            source += Code.ReturnTypeCode.Replace(Tag.Class, className, StringComparison.Ordinal);

            var usedTypes = handlerMaps.SelectMany(x => new[] { x.HandlerType, x.Key.RequestType, x.Key.ResponseType })
                .Concat(new[] { serviceBaseType })
                .ToArray();

            return new GenerateSourceResult(source, usedTypes);
        }

        private static string GetFullClassName(Type serviceBaseType)
        {
            return GlobalPrefix + serviceBaseType.FullName.Replace('+', '.');
        }

        private static string GetGeneratedName(Type serviceBaseType)
        {
            var className = serviceBaseType.Name;

            if (className.EndsWith(Base, StringComparison.Ordinal)) // which it does from the protobuf code generation
                className = className.Substring(0, className.Length - Base.Length);

            className += Dispatcher;

            return className;
        }

        private string GenerateSource(IEnumerable<DispatcherMap> handlerMaps, string className, string baseClassName)
        {
            var attributes = GenerateAttributes(_configuration.GlobalAuthorizeAttributes);

            var members = Code.ConstructorCode
                .Replace(Tag.Constructor, className, StringComparison.Ordinal)
                +
                string.Concat(handlerMaps.Select(GenerateMethod));

            var classSource = Code.ClassCode
                .Replace(Tag.Attributes, attributes, StringComparison.Ordinal)
                .Replace(Tag.Class, className, StringComparison.Ordinal)
                .Replace(Tag.BaseClass, baseClassName, StringComparison.Ordinal)
                .Replace(Tag.Members, members, StringComparison.Ordinal);

            return classSource;
        }

        private string GenerateMethod(DispatcherMap definition)
        {
            var attributes = GenerateAttributes(definition.AuthorizeAttributes);

            var method = definition.Key.ResponseType == typeof(void)
                ? Code.MethodCommandHandlerCode
                : Code.MethodHandlerCode;

            return method
                .Replace(Tag.Attributes, attributes, StringComparison.Ordinal)
                .Replace(Tag.Method, definition.Key.MethodName, StringComparison.Ordinal)
                .Replace(Tag.Request, GetFullClassName(definition.Key.RequestType), StringComparison.Ordinal)
                .Replace(Tag.Response, GetFullClassName(definition.Key.ResponseType), StringComparison.Ordinal)
                .Replace(Tag.Handler, GetFullClassName(definition.HandlerType), StringComparison.Ordinal);
        }

        private static string GenerateAttributes(IReadOnlyCollection<AuthorizeAttribute> authorizeAttributes)
        {
            var lines = authorizeAttributes.Select(a => Code.AuthorizeAttributeCode
                .Replace(Tag.AttributeProperties, GenerateAttributeProperties(a), StringComparison.Ordinal));

            return string.Concat(lines);
        }

        private static string GenerateAttributeProperties(AuthorizeAttribute attribute)
        {
            var properties = new List<string>(3);

            if (attribute.Roles != null)
                properties.Add(Code.RolesPropertyCode.Replace(Tag.Roles, attribute.Roles, StringComparison.Ordinal));

            if (attribute.Policy != null)
                properties.Add(Code.PolicyPropertyCode.Replace(Tag.Policy, attribute.Policy, StringComparison.Ordinal));

            if (attribute.AuthenticationSchemes != null)
                properties.Add(Code.AuthenticationSchemesPropertyCode.Replace(Tag.AuthenticationSchemes, attribute.AuthenticationSchemes, StringComparison.Ordinal));

            return string.Join(", ", properties);
        }

        private IEnumerable<DispatcherMap> GetMethodsToImplement(Type serviceBase)
        {
            var methods = serviceBase.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            // methods that can be implemented have
            // - return type of Task<T> where T is the type of the response
            // - 2 arguments where the 
            //   - first is the actual argument
            //   - second is of type ServerCallContext

            foreach (var method in methods)
            {
                if (method.ReturnType == typeof(void))
                    continue;

                var parameters = method.GetParameters();

                if (parameters.Length != 2) // one parameter + ServerCallContext
                    continue;

                if (parameters[1].ParameterType != typeof(ServerCallContext))
                    continue;

                var returnTaskType = method.ReturnType; // returnTaskType is Task<T> where T is the actual return type
                if (!returnTaskType.IsGenericType)
                    continue;

                if (returnTaskType.GetGenericTypeDefinition() != typeof(Task<>))
                    continue;

                var methodName = method.Name;
                var parameterType = parameters[0].ParameterType;
                var returnType = returnTaskType.GetGenericArguments().Single(); // get the actual return type

                var handlerMap = _handlerStore.FindHandlerMap(methodName, parameterType, returnType);
                if (handlerMap == null)
                {
                    if (returnType != typeof(void))
                        throw new InvalidOperationException($"Couldn't find a type that implements [IHandler<{parameterType.Name}, {returnType.Name}>] to handle method [{Format.Method(methodName, parameterType, returnType)}]");
                    else
                        throw new InvalidOperationException($"Couldn't find a type that implements [IHandler<{parameterType.Name}, {returnType.Name}>] or [ICommandHandler<{parameterType.Name}>] to handle method [{Format.Method(methodName, parameterType, returnType)}]");
                }

                yield return handlerMap;
            }
        }
    }
}
