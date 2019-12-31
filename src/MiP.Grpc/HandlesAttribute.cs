using System;

namespace MiP.Grpc
{
    /// <summary>
    /// Used on a class to set the name of the service method that is handled by the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class HandlesAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the service method that is handled by the class that has this attribute.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlesAttribute"/> attribute.
        /// </summary>
        public HandlesAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlesAttribute"/> attribute.
        /// </summary>
        /// <param name="methodName">Name of the service method that is handled by the class that has this attribute.</param>
        public HandlesAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}