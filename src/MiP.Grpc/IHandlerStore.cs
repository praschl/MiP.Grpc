using System;

namespace MiP.Grpc
{
    internal interface IHandlerStore
    {
        Type FindHandler(string methodName, Type parameterType, Type returnTypeArgument);
    }
}