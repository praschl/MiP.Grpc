using Grpc.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    /// <summary>
    /// Used by generated code to call the actual handler method.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Dispatches a "query" like method to a handler and returns it's result.
        /// When called from the generated code, the code passes type parameters to determine the handler that should be invoked.
        /// The handler is resolved from the <see cref="IServiceProvider"/> of the current scope. 
        /// The RunAsync() method is invoked and the result is returned.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request parameter of the handler and service method being handled.</typeparam>
        /// <typeparam name="TResponse">The return type of the handler and service method being handled.</typeparam>
        /// <typeparam name="THandler">
        /// The handler that should be resolved from the <see cref="IServiceProvider"/>. This should be a concrete class.
        /// </typeparam>
        /// <param name="request">The request parameter from the service method being handled.</param>
        /// <param name="context">The <see cref="ServerCallContext"/> parameter from the service method being handled.</param>
        /// <param name="methodName">
        /// The actual name of the method being handled. 
        /// Currently this parameter is not used by the default implementation, but may be useful for custom implementations.
        /// </param>
        /// <returns>A <see cref="Task"/> that once completed will contain the result of the method.</returns>
        Task<TResponse> Dispatch<TRequest, TResponse, THandler>(TRequest request, ServerCallContext context, [CallerMemberName] string methodName = null)
            where THandler : IHandler<TRequest, TResponse>;
    }
}
