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
using MiP.Grpc.CodeGen;

namespace MiP.Grpc
{
    internal class DispatcherCompiler
    {
        private readonly MapGrpcServiceConfiguration _configuration;
        private readonly MethodGenerator _methodGenerator;

        public DispatcherCompiler(IHandlerStore handlerStore, MapGrpcServiceConfiguration configuration)
        {
            _configuration = configuration;

            var methodCodeGenerator = new MethodCodeGenerator();

            // build a chain of responsibility
            var bidiStream = new BidiStreamMethodGenerator(methodCodeGenerator, handlerStore);
            var clientStream = new ClientStreamMethodGenerator(methodCodeGenerator, handlerStore);
            var serverStream = new ServerStreamMethodGenerator(methodCodeGenerator, handlerStore);
            var commandQuery = new CommandQueryMethodGenerator(methodCodeGenerator, handlerStore);

            bidiStream.NextGenerator = clientStream;
            clientStream.NextGenerator = serverStream;
            serverStream.NextGenerator = commandQuery;

            _methodGenerator = bidiStream;
        }

        public Type CompileDispatcher(Type serviceBaseType)
        {
            try
            {
                var result = GenerateScript(serviceBaseType);

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
                    typeof(ServerCallContext),
                    typeof(AuthorizeAttribute),
                    typeof(Proto.Empty)
                }.Concat(importTypes);

                IEnumerable<Assembly> references = types
                            //.Where(t => !string.IsNullOrEmpty(t.Assembly.Location))
                            .Select(t => t.Assembly)
                            .Distinct();

                var compiledType = CSharpScript.EvaluateAsync<Type>(source,
                    ScriptOptions.Default
                        .WithReferences(references))
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

        private GenerateSourceResult GenerateScript(Type serviceBaseType)
        {
            var methodInfos = GetMethodsToImplement(serviceBaseType);

            var className = serviceBaseType.GetGeneratedDispatcherName();

            var baseClassName = serviceBaseType.GetFullClassName();

            // get source of the class with all its methods
            var source = GenerateClassSource(methodInfos, className, baseClassName);

            // add a line which returns the Type of that class from the script.
            source += Code.ReturnTypeCode.Replace(Tag.Class, className, StringComparison.Ordinal);

            var usedTypes = methodInfos.SelectMany(x => new[] { x.Handler, x.Request, x.Response })
                .Concat(new[] { serviceBaseType })
                .ToArray();

            return new GenerateSourceResult(source, usedTypes);
        }

        private string GenerateClassSource(IEnumerable<GeneratedMethod> methods, string className, string baseClassName)
        {
            var attributes = AttributeCodeGenerator.GenerateAttributes(_configuration.GlobalAuthorizeAttributes);

            var members = Code.ConstructorCode
                .Replace(Tag.Constructor, className, StringComparison.Ordinal)
                +
                string.Concat(methods.Select(m => m.Code));

            var classSource = Code.ClassCode
                .Replace(Tag.Attributes, attributes, StringComparison.Ordinal)
                .Replace(Tag.Class, className, StringComparison.Ordinal)
                .Replace(Tag.BaseClass, baseClassName, StringComparison.Ordinal)
                .Replace(Tag.Members, members, StringComparison.Ordinal);

            return classSource;
        }

        private IEnumerable<GeneratedMethod> GetMethodsToImplement(Type serviceBase)
        {
            var methods = serviceBase.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();

                if (method.ReturnType == typeof(void))
                    continue;

                if (parameters.Length == 0)
                    continue;

                var generatedMethod = _methodGenerator.Generate(serviceBase, method);
                if (generatedMethod != null)
                    yield return generatedMethod;
            }
        }
    }
}
