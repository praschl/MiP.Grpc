using System;

namespace MiP.Grpc
{
    internal interface IHandlerStore
    {
        DispatcherMap FindHandlerMap(string methodName, Type parameterType, Type returnTypeArgument, Type serviceBase);
    }
}