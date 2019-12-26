using System;

namespace MiP.Grpc
{
    internal class DispatcherMap
    {
        public DispatcherMap(DispatcherMapKey key, Type handlerType)
        {
            Key = key;
            HandlerType = handlerType;
        }

        public DispatcherMapKey Key { get; }
        public Type HandlerType { get; }
    }
}