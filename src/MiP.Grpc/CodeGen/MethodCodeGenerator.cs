using System;
using MiP.Grpc.CodeGen;

namespace MiP.Grpc
{
    internal class MethodCodeGenerator : IMethodCodeGenerator
    {
        public GeneratedMethod GenerateMethod(string codeTemplate, string methodName, Type request, Type response, HandlerInfo handlerInfo)
        {
            var attributes = AttributeCodeGenerator.GenerateAttributes(handlerInfo.AuthorizeAttributes);

            string code = codeTemplate
                .Replace(Tag.Attributes, attributes, StringComparison.Ordinal)
                .Replace(Tag.Method, methodName, StringComparison.Ordinal)
                .Replace(Tag.Request, request.GetFullClassName(), StringComparison.Ordinal)
                .Replace(Tag.Response, response.GetFullClassName(), StringComparison.Ordinal)
                .Replace(Tag.Handler, handlerInfo.Handler.GetFullClassName(), StringComparison.Ordinal);

            return new GeneratedMethod(handlerInfo.Handler, request, response, code);
        }
    }
}
