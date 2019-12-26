using System;
using System.Collections.Generic;

namespace MiP.Grpc
{
    internal struct DispatcherMapKey : IEquatable<DispatcherMapKey>
    {
        public DispatcherMapKey(string methodName, Type serviceType)
        {
            MethodName = methodName;
            ServiceType = serviceType;
        }

        public string MethodName { get; }
        public Type ServiceType { get; }

        public override bool Equals(object obj)
        {
            return obj is DispatcherMapKey key &&
                   MethodName == key.MethodName &&
                   EqualityComparer<Type>.Default.Equals(ServiceType, key.ServiceType);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MethodName, ServiceType);
        }

        public bool Equals(DispatcherMapKey other)
        {
            return Equals((object)other);
        }
    }
}