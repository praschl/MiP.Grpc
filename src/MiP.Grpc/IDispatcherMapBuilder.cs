using System;

namespace MiP.Grpc
{
    public interface IDispatcherMapBuilder
    {
        IDispatcherMapBuilder Add<THandler>(string name);

        IDispatcherMapBuilder Add(Type handlerType, string name);
    }
}