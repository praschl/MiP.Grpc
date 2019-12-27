using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MiP.Grpc
{
    internal struct DispatcherMapKey : IEquatable<DispatcherMapKey>
    {
        public DispatcherMapKey(string methodName, Type requestType, Type responseType)
        {
            MethodName = methodName;
            RequestType = requestType;
            ResponseType = responseType;
        }

        public string MethodName { get; }
        public Type RequestType { get; }
        public Type ResponseType { get; }

        public override bool Equals(object obj)
        {
            return obj is DispatcherMapKey key && Equals(key);
        }

        public bool Equals([AllowNull] DispatcherMapKey other)
        {
            return MethodName == other.MethodName &&
                   EqualityComparer<Type>.Default.Equals(RequestType, other.RequestType) &&
                   EqualityComparer<Type>.Default.Equals(ResponseType, other.ResponseType);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MethodName, RequestType, ResponseType);
        }
    }
}