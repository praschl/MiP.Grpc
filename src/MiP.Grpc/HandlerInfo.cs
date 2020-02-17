using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace MiP.Grpc
{
    internal class HandlerInfo
    {
        public HandlerInfo(Type handler, IReadOnlyCollection<AuthorizeAttribute> authorizeAttributes)
        {
            Handler = handler;
            AuthorizeAttributes = authorizeAttributes ?? Array.Empty<AuthorizeAttribute>();
        }

        public Type Handler { get; }
        public IReadOnlyCollection<AuthorizeAttribute> AuthorizeAttributes { get; }
    }
}