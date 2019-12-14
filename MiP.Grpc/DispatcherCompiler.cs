using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace MiP.Grpc
{
    public class DispatcherCompiler
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
        }

        private const string Base = "Base";
        public const string ClassCode = @"
public class {Class} : {BaseClass}
{{Members}}
";

        public const string ConstructorCode = @"
    private readonly IDispatcher _dispatcher;
    public {Constructor}(IDispatcher dispatcher, IServiceProvider serviceProvider)
    {
        _dispatcher = dispatcher ?? new Dispatcher(serviceProvider);
    }
";

        public const string MethodCode = @"
    public async override Task<{Response}> {Method}({Request} request, ServerCallContext context)
    {
        return await _dispatcher.Dispatch<{Request}, {Response}>(request, context);

        //var query = (IQuery<{Request}, {Response}>) _serviceProvider.GetService(typeof(IQuery<{Request}, {Response}>));

        //if (query == null)
        //    throw new InvalidOperationException(""IQuery<{Request}, {Response}> could not be resolved."");

        //return await query.RunAsync(request, context);
    }
";

        public Type CompileDispatcher(Type serviceBase)
        {
            string source = GenerateSource(serviceBase);

            var result = CompileToType(source, serviceBase);

            return result;
        }

        private static Type CompileToType(string source, Type serviceBase)
        {
            try
            {
                var type = CSharpScript.EvaluateAsync<Type>(source,
                    ScriptOptions.Default
                        .WithReferences(
                            serviceBase.Assembly,
                            typeof(IServiceProvider).Assembly,
                            typeof(Task<>).Assembly,
                            typeof(IQuery<,>).Assembly,
                            typeof(ServerCallContext).Assembly
                            )
                        .WithImports(
                            serviceBase.Namespace,
                            typeof(IServiceProvider).Namespace,
                            typeof(Task<>).Namespace,
                            typeof(IQuery<,>).Namespace,
                            typeof(ServerCallContext).Namespace
                            )
                    )
                    .Result;

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

        private string GenerateSource(Type serviceBase)
        {
            var definitions = GetMethodsToImplement(serviceBase);
            var implName = serviceBase.Name;
            if (implName.EndsWith(Base))
                implName = implName.Substring(0, implName.Length - Base.Length);
            implName += "Dispatcher";

            var baseName = serviceBase.FullName.Replace("+", ".");

            var source = GenerateSource(definitions, implName, baseName);

            source += Environment.NewLine + Environment.NewLine
                + "return typeof(" + implName + ");";
            return source;
        }

        private string GenerateSource(IEnumerable<QueryDefinition> definitions, string typeName, string baseClass)
        {
            var members =
                ConstructorCode.Replace(Tag.Constructor, typeName)
                +
                string.Concat(definitions.Select(GenerateMethod));

            var classSource = ClassCode
                .Replace(Tag.Class, typeName)
                .Replace(Tag.BaseClass, baseClass)
                .Replace(Tag.Members, members);

            return classSource;
        }

        private string GenerateMethod(QueryDefinition definition)
        {
            return MethodCode
                .Replace(Tag.Method, definition.MethodName)
                .Replace(Tag.Request, definition.RequestType.Name)
                .Replace(Tag.Response, definition.ResponseType.Name);
        }

        private static IEnumerable<QueryDefinition> GetMethodsToImplement(Type serviceBase)
        {
            var methods = serviceBase.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (method.ReturnType == typeof(void))
                    continue;

                var parameters = method.GetParameters();

                if (parameters.Length != 2)
                    continue;

                if (parameters[1].ParameterType != typeof(ServerCallContext))
                    continue;

                var returnType = method.ReturnType;
                if (!returnType.IsGenericType)
                    continue;

                if (returnType.GetGenericTypeDefinition() != typeof(Task<>))
                    continue;

                var returnTypeArgument = returnType.GetGenericArguments().Single();

                var result = new QueryDefinition(method.Name, parameters[0].ParameterType, returnTypeArgument);
                yield return result;
            }
        }
    }
}

