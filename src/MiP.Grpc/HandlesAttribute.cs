using System;

namespace MiP.Grpc
{
    /// <summary>
    /// Used on a class or method to specify service and method that is handled by the class or method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class HandlesAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the service method that is handled by the class or method that has this attribute set.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the type of the service base whose method should be handled by the class or method that has this attribute set.
        /// </summary>
        public Type ServiceBase { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlesAttribute"/> attribute.
        /// </summary>
        public HandlesAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlesAttribute"/> attribute.
        /// </summary>
        /// <param name="methodName">Name of the service method that is handled by the class or method  that has this attribute.</param>
        /// <param name="serviceBase">The type of the service base whose method should be handled by the class or method that has this attribute set.</param>
        public HandlesAttribute(string methodName, Type serviceBase = null)
        {
            MethodName = methodName;
            ServiceBase = serviceBase;
        }
    }
}