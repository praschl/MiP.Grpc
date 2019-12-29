using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    /// <summary>
    /// Implemented to make a class handle a specific service method.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request parameter of the method being handled.</typeparam>
    /// <typeparam name="TResponse">The type of the response of the method being handled.</typeparam>
    public interface IHandler<TRequest, TResponse>
    {
        /// <summary>
        /// Runs the command or query, passes the <paramref name="request"/> and <paramref name="context"/> and returns the result of the method.
        /// </summary>
        /// <param name="request">The request parameter.</param>
        /// <param name="context">The <see cref="ServerCallContext"/>.</param>
        /// <returns>A <see cref="Task"/> that will contain the result of the handler.</returns>
        Task<TResponse> RunAsync(TRequest request, ServerCallContext context);
    }
}
