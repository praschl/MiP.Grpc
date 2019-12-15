using System;

namespace MiP.Grpc
{
    internal class QueryDefinition
    {
        public QueryDefinition(string methodName, Type requestType, Type responseType)
        {
            MethodName = methodName;
            RequestType = requestType;
            ResponseType = responseType;
        }

        public string MethodName { get; }
        public Type RequestType { get; }
        public Type ResponseType { get; }
    }
}
