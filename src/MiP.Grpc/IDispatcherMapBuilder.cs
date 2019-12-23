using System;
using System.Reflection;

namespace MiP.Grpc
{
    public interface IDispatcherMapBuilder
    {
        IDispatcherMapBuilder Add<THandler>(string name);

        IDispatcherMapBuilder Add(Type handlerType, string name);

        IDispatcherMapBuilder Add(Assembly assembly);

        Type FindHandler(string methodName, Type parameterType, Type returnTypeArgument);
    }
}