using System;
using System.Reflection;
using MiP.Grpc.CodeGen;

namespace MiP.Grpc
{
    internal abstract class MethodGenerator
    {
        private readonly IMethodCodeGenerator _methodCodeGenerator;

        public MethodGenerator(IMethodCodeGenerator methodCodeGenerator)
        {
            _methodCodeGenerator = methodCodeGenerator;
        }

        public MethodGenerator NextGenerator { get; set; }

        public GeneratedMethod Generate(Type serviceBase, MethodInfo method)
        {
            if (!CanGenerate(method))
                return NextGenerator?.Generate(serviceBase, method);

            return GenerateCode(serviceBase, method);
        }

        protected abstract bool CanGenerate(MethodInfo method);

        protected abstract GeneratedMethod GenerateCode(Type serviceBase, MethodInfo method);

        protected GeneratedMethod GenerateMethod(string codeTemplate, string methodName, Type request, Type response, HandlerInfo handlerInfo)
        {
            return _methodCodeGenerator.GenerateMethod(codeTemplate, methodName, request, response, handlerInfo);
        }
    }
}
