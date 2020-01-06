using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MiP.Grpc
{
    internal struct MapKey : IEquatable<MapKey>
    {
        private readonly Type _serviceBase;
        private readonly Type _handlerInterface;
        private readonly string _methodName;

        public MapKey(Type serviceBase, Type handlerInterface, string methodName)
        {
            _serviceBase = serviceBase;
            _handlerInterface = handlerInterface;
            _methodName = methodName;
        }

        public override bool Equals(object obj)
        {
            return obj is MapKey key && Equals(key);
        }

        public bool Equals([AllowNull] MapKey other)
        {
            return EqualityComparer<Type>.Default.Equals(_serviceBase, other._serviceBase)
                   &&
                   EqualityComparer<Type>.Default.Equals(_handlerInterface, other._handlerInterface)
                   &&
                   _methodName == other._methodName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_serviceBase, _handlerInterface, _methodName);
        }
    }
}