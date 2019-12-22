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

        private IDispatcherMapBuilder _dispatcherMap;

        public DispatcherCompiler(IDispatcherMapBuilder dispatcherMap)
        {
            _dispatcherMap = dispatcherMap;
        }

        public Type CompileDispatcher(Type serviceBaseType)
        {
            var (source, types) = GenerateSource(serviceBaseType);

            var result = CompileToType(source, types);

            return result;
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

                var type = CSharpScript.EvaluateAsync<Type>(source,
                    ScriptOptions.Default
                        .WithReferences(
                            types.Select(t => t.Assembly).Distinct()
                            )
                        .WithImports(
                            types.Select(t => t.Namespace).Distinct()
                            )
                    )
                    .GetAwaiter().GetResult();

                return type;
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

        private (string source, IReadOnlyCollection<Type> usedTypes) GenerateSource(Type serviceBase)
        {
            var definitions = GetMethodsToImplement(serviceBase);

            var implName = GetClassName(serviceBase);

            var baseName = serviceBase.FullName.Replace("+", ".", StringComparison.Ordinal); // + is used by framework for nested classes

            var source = GenerateSource(definitions, implName, baseName);

            source += ReturnTypeCode.Replace(Tag.Class, implName, StringComparison.Ordinal);

            var usedTypes = definitions.SelectMany(x => new[] { x.ConcreteHandlerType, x.RequestType, x.ResponseType })
                .Concat(new[] { serviceBase })
                .ToArray();

            return (source, usedTypes);
        }

        private static string GetClassName(Type serviceBase)
        {
            var implName = serviceBase.Name;

            if (implName.EndsWith(Base, StringComparison.Ordinal)) // which it does from the protobuf code generation
                implName = implName.Substring(0, implName.Length - Base.Length);

            implName += Dispatcher;

            return implName;
        }

        private string GenerateSource(IEnumerable<QueryDefinition> definitions, string typeName, string baseClass)
        {
            var members =
                ConstructorCode.Replace(Tag.Constructor, typeName, StringComparison.Ordinal)
                +
                string.Concat(definitions.Select(GenerateMethod));

            var classSource = ClassCode
                .Replace(Tag.Class, typeName, StringComparison.Ordinal)
                .Replace(Tag.BaseClass, baseClass, StringComparison.Ordinal)
                .Replace(Tag.Members, members, StringComparison.Ordinal);

            return classSource;
        }

        private string GenerateMethod(QueryDefinition definition)
        {
            return MethodCode
                .Replace(Tag.Method, definition.MethodName, StringComparison.Ordinal)
                .Replace(Tag.Request, definition.RequestType.Name, StringComparison.Ordinal)
                .Replace(Tag.Response, definition.ResponseType.Name, StringComparison.Ordinal)
                .Replace(Tag.Handler, definition.ConcreteHandlerType.Name, StringComparison.Ordinal);
        }

        private IEnumerable<QueryDefinition> GetMethodsToImplement(Type serviceBase)
        {
            var methods = serviceBase.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

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

                var handlerType = _dispatcherMap.FindHandler(methodName, parameterType, returnType);
                if (handlerType == null)
                    throw new InvalidOperationException($"No implementation found for IHandler<{parameterType.Name}, {returnType.Name}>");

                var result = new QueryDefinition(methodName, parameterType, returnType, handlerType);
                yield return result;
            }
        }
    }
}

