using System;

namespace MiP.Grpc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HandlesAttribute : Attribute
    {
        public string MethodName { get; set; }

        public HandlesAttribute()
        {
        }

        public HandlesAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}