using System;
using MiP.Grpc.CodeGen;

namespace MiP.Grpc
{
    internal interface IMethodCodeGenerator
    {
        GeneratedMethod GenerateMethod(string codeTemplate, string methodName, Type request, Type response, HandlerInfo handlerInfo);
    }
}
