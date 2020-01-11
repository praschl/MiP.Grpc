using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MiP.Grpc
{
    internal struct DispatcherMapKey : IEquatable<DispatcherMapKey>
    {
        public DispatcherMapKey(string methodName, Type requestType, Type responseType, Type serviceBase)
        {
            MethodName = methodName;
            RequestType = requestType;
            ResponseType = responseType;
            ServiceBase = serviceBase;
        }

        public string MethodName { get; }
        public Type RequestType { get; }
        public Type ResponseType { get; }
        public Type ServiceBase { get; }

        public override bool Equals(object obj)
        {
            return obj is DispatcherMapKey key && Equals(key);
        }

        public bool Equals([AllowNull] DispatcherMapKey other)
        {
            return MethodName == other.MethodName &&
                   EqualityComparer<Type>.Default.Equals(RequestType, other.RequestType) &&
                   EqualityComparer<Type>.Default.Equals(ResponseType, other.ResponseType) &&
                   EqualityComparer<Type>.Default.Equals(ServiceBase, other.ServiceBase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MethodName, RequestType, ResponseType, ServiceBase);
        }
    }
}