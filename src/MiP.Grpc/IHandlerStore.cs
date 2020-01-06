using System;

namespace MiP.Grpc
{
    internal interface IHandlerStore
    {
        HandlerInfo Find(Type serviceBase, Type handlerInterface, string methodName);
    }
}