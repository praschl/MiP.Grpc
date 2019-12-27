using System;
using System.Collections.Generic;
using System.Linq;

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

        public string GetPreferredName()
        {
            var attribute = (HandlesAttribute)Implementation.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(HandlesAttribute));
            if (attribute != null)
                return attribute.MethodName;

            // check class name
            string name = Implementation.Name;

            string interfaceNameWithoutPrefix = nameof(IHandler<object, object>).Substring(1);

            if (name.EndsWith(interfaceNameWithoutPrefix, StringComparison.Ordinal))
                name = name.Substring(0, name.Length - interfaceNameWithoutPrefix.Length);

            return name;
        }
    }
}