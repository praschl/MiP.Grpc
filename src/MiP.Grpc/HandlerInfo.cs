using System;
using System.Collections.Generic;

namespace MiP.Grpc
{
    internal class HandlerInfo
    {
        public Type Implementation { get; set; }
        public IReadOnlyList<Type> ServiceTypes { get; set; }

        public HandlerInfo(Type implementation, IReadOnlyList<Type> serviceTypes)
        {
            Implementation = implementation;
            ServiceTypes = serviceTypes;
        }

        public string GetPreferredName()
        {
            // TODO: check attributes

            // check class name
            string name = Implementation.Name;

            string interfaceNameWithoutPrefix = nameof(IHandler<object, object>).Substring(1);

            if (name.EndsWith(interfaceNameWithoutPrefix, StringComparison.Ordinal))
                name = name.Substring(0, name.Length - interfaceNameWithoutPrefix.Length);

            return name;
        }
    }
}