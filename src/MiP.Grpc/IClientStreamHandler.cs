using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    /// <summary>
    /// Implemented to make a class handle a specific service method
    /// that streams objects from the client to the service 
    /// and returns a response when done.
    /// </summary>
    /// <typeparam name="TStreamRequest">Type of the objects streamed from the client to the server.</typeparam>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    public interface IClientStreamHandler<TStreamRequest, TResponse>
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="requestStream">The stream of request objects from the client.</param>
        /// <param name="context">The <see cref="ServerCallContext"/>.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task<TResponse> RunAsync(IAsyncStreamReader<TStreamRequest> requestStream, ServerCallContext context);
    }
}
