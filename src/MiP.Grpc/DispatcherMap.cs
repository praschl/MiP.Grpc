using System;

namespace MiP.Grpc
{
    internal class DispatcherMap
    {
        public DispatcherMap(string methodName, Type handlerType, Type serviceType)
        {
            MethodName = methodName;
            HandlerType = handlerType;
            ServiceType = serviceType;
        }

        public string MethodName { get; }
        public Type HandlerType { get; }
        public Type ServiceType { get; }
    }
}