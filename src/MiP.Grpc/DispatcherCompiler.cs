using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Proto = Google.Protobuf.WellKnownTypes;

namespace MiP.Grpc
{
    internal class DispatcherCompiler
    {
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
        }

        private const string Base = "Base";
        private const string Dispatcher = "Dispatcher";

        public const string ClassCode = @"
public class {Class} : {BaseClass}
{{Members}}
";

        public const string ConstructorCode = @"
    private readonly IDispatcher _dispatcher;
    public {Constructor}(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
";

        public const string MethodCode = @"
    public async override Task<{Response}> {Method}({Request} request, ServerCallContext context)
    {
        return await _dispatcher.Dispatch<{Request}, {Response}, {Handler}>(request, context);
    }
";

        public const string ReturnTypeCode = @"
return typeof({Class});
";

        public const string HandlerTypeFormat = "IHandler<{Request}, {Response}>";

        private readonly IHandlerStore _handlerStore;

        public DispatcherCompiler(IHandlerStore handlerStore)
        {
            _handlerStore = handlerStore;
        }

        public Type CompileDispatcher(Type serviceBaseType)
        {
            try
            {
                var (source, types) = GenerateSource(serviceBaseType);

                var dispatcherType = CompileToType(source, types);

                return dispatcherType;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Generating implementation for service {serviceBaseType.FullName} failed", ex);
            }
        }

        private static Type CompileToType(string source, IEnumerable<Type> importTypes)
        {
            try
            {
                var types = new[]
                {
                    typeof(IServiceProvider),
                    typeof(Task<>),
                    typeof(IHandler<,>),
                    typeof(ServerCallContext),
                    typeof(Proto.Empty)
                }.Concat(importTypes);

                var compiledType = CSharpScript.EvaluateAsync<Type>(source,
                    ScriptOptions.Default
                        .WithReferences(
                            types.Select(t => t.Assembly).Distinct()
                            )
                        .WithImports(
                            types.Select(t => t.Namespace).Distinct()
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

        private (string source, IReadOnlyCollection<Type> usedTypes) GenerateSource(Type serviceBaseType)
        {
            var methodHandlerDefinitions = GetMethodsToImplement(serviceBaseType);

            var className = GetClassName(serviceBaseType);

            var baseClassName = serviceBaseType.FullName.Replace("+", ".", StringComparison.Ordinal); // + is used by framework for nested classes

            var source = GenerateSource(methodHandlerDefinitions, className, baseClassName);

            source += ReturnTypeCode.Replace(Tag.Class, className, StringComparison.Ordinal);

            var usedTypes = methodHandlerDefinitions.SelectMany(x => new[] { x.HandlerType, x.RequestType, x.ResponseType })
                .Concat(new[] { serviceBaseType })
                .ToArray();

            return (source, usedTypes);
        }

        private static string GetClassName(Type serviceBaseType)
        {
            var className = serviceBaseType.Name;

            if (className.EndsWith(Base, StringComparison.Ordinal)) // which it does from the protobuf code generation
                className = className.Substring(0, className.Length - Base.Length);

            className += Dispatcher;

            return className;
        }

        private string GenerateSource(IEnumerable<MethodHandlerDefinition> definitions, string className, string baseClassName)
        {
            var members =
                ConstructorCode.Replace(Tag.Constructor, className, StringComparison.Ordinal)
                +
                string.Concat(definitions.Select(GenerateMethod));

            var classSource = ClassCode
                .Replace(Tag.Class, className, StringComparison.Ordinal)
                .Replace(Tag.BaseClass, baseClassName, StringComparison.Ordinal)
                .Replace(Tag.Members, members, StringComparison.Ordinal);

            return classSource;
        }

        private string GenerateMethod(MethodHandlerDefinition definition)
        {
            return MethodCode
                .Replace(Tag.Method, definition.MethodName, StringComparison.Ordinal)
                .Replace(Tag.Request, definition.RequestType.Name, StringComparison.Ordinal)
                .Replace(Tag.Response, definition.ResponseType.Name, StringComparison.Ordinal)
                .Replace(Tag.Handler, definition.HandlerType.Name, StringComparison.Ordinal);
        }

        private IEnumerable<MethodHandlerDefinition> GetMethodsToImplement(Type serviceBase)
        {
            var methods = serviceBase.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            // methods that can be implemented have
            // - return type of Task<T>
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

                string methodName = method.Name;
                Type parameterType = parameters[0].ParameterType;
                var returnType = returnTaskType.GetGenericArguments().Single(); // get the actual return type

                var handlerType = _handlerStore.FindHandler(methodName, parameterType, returnType);
                if (handlerType == null)
                    throw new InvalidOperationException($"Couldn't find a type that implements IHandler<{parameterType.Name}, {returnType.Name}> to handle method {returnType.Name} {methodName}({parameterType.Name}, ServerCallContext)");

                var result = new MethodHandlerDefinition(methodName, parameterType, returnType, handlerType);
                yield return result;
            }
        }
    }
}

