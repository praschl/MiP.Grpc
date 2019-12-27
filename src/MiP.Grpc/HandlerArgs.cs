using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MiP.Grpc
{
    internal struct HandlerArgs : IEquatable<HandlerArgs>
    {
        public HandlerArgs(Type requestType, Type responseType)
        {
            RequestType = requestType;
            ResponseType = responseType;
        }

        public Type RequestType { get; }
        public Type ResponseType { get; }

        public override bool Equals(object obj)
        {
            return obj is HandlerArgs args && Equals(args);
        }

        public bool Equals([AllowNull] HandlerArgs other)
        {
            return EqualityComparer<Type>.Default.Equals(RequestType, other.RequestType) &&
                   EqualityComparer<Type>.Default.Equals(ResponseType, other.ResponseType);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RequestType, ResponseType);
        }
    }
}