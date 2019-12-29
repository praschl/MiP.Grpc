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

            // remove "CommandHandler" from end
            name = TrimEnd(name, nameof(ICommandHandler<object>).Substring(1));

            // remove "Handler" from end
            name = TrimEnd(name, nameof(IHandler<object, object>).Substring(1));

            return name;
        }

        private static string TrimEnd(string from, string trim)
        {
            if (from.EndsWith(trim, StringComparison.Ordinal))
                return from.Substring(0, from.Length - trim.Length);

            return from;
        }
    }
}