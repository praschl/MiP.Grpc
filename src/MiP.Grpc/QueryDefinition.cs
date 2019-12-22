using System;

namespace MiP.Grpc
{
    internal class QueryDefinition
    {
        public QueryDefinition(string methodName, Type requestType, Type responseType, Type concreteHandlerType)
        {
            MethodName = methodName;
            RequestType = requestType;
            ResponseType = responseType;
            ConcreteHandlerType = concreteHandlerType;
        }

        public string MethodName { get; }
        public Type RequestType { get; }
        public Type ResponseType { get; }
        public Type ConcreteHandlerType { get; }
    }
}
