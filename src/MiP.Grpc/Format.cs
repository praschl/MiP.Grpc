using System;

namespace MiP.Grpc
{
    internal static class Format
    {
        public static string Method(string name, Type requestType, Type returnType)
        {
            return $"{returnType.Name} {name}({requestType.Name}, ServerCallContext)";
        }
    }
}
