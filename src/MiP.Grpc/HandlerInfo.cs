using System;
using System.Collections.Generic;

namespace MiP.Grpc
{
    internal class HandlerInfo
    {
        public Type Implementation { get; set; }
        public IReadOnlyList<HandlerArgs> ServiceArgs { get; set; }

        public HandlerInfo(Type implementation, IReadOnlyList<HandlerArgs> serviceTypes)
        {
            Implementation = implementation;
            ServiceArgs = serviceTypes;
        }
    }
}