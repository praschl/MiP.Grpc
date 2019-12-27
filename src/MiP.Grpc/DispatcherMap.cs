using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace MiP.Grpc
{
    internal class DispatcherMap
    {
        public DispatcherMap(DispatcherMapKey key, Type handlerType, IReadOnlyCollection<AuthorizeAttribute> authorizeAttributes)
        {
            Key = key;
            HandlerType = handlerType;
            AuthorizeAttributes = authorizeAttributes;
        }

        public DispatcherMapKey Key { get; }
        public Type HandlerType { get; }
        public IReadOnlyCollection<AuthorizeAttribute> AuthorizeAttributes { get; }
    }
}