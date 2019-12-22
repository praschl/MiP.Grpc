using System;

namespace MiP.Grpc
{
    internal class MethodHandlerDefinition
    {
        public MethodHandlerDefinition(string methodName, Type requestType, Type responseType, Type handlerType)
        {
            MethodName = methodName;
            RequestType = requestType;
            ResponseType = responseType;
            HandlerType = handlerType;
        }

        public string MethodName { get; }
        public Type RequestType { get; }
        public Type ResponseType { get; }
        public Type HandlerType { get; }
    }
}
