using System;
using System.Collections.Generic;

namespace MiP.Grpc
{
    internal class GenerateSourceResult
    {
        public GenerateSourceResult(string source, IReadOnlyCollection<Type> usedTypes)
        {
            Source = source;
            UsedTypes = usedTypes;
        }

        public string Source { get; }
        public IReadOnlyCollection<Type> UsedTypes { get; }
    }
}
