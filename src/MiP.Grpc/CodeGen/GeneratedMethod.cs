using System;

namespace MiP.Grpc.CodeGen
{
    internal class GeneratedMethod
    {
        public GeneratedMethod(Type handler, Type request, Type response, string code)
        {
            Handler = handler;
            Request = request;
            Response = response;
            Code = code;
        }

        public Type Handler { get; }
        public Type Request { get; }
        public Type Response { get; }
        public string Code { get; }
    }
}
