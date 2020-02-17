using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    /// <summary>
    /// Implemented to make a class handle a specific service method
    /// that sends a request to the service and then gets objects streamed from the server.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TStreamResponse">The type of the objects streamed from the server to the client.</typeparam>
    public interface IServerStreamHandler<TRequest, TStreamResponse>
    {
        /// <summary>
        /// Handles the stream request by taking the request and sending back a stream of <typeparamref name="TStreamResponse"/>.
        /// </summary>
        /// <param name="streamRequest">
        /// An instance of <see cref="IServerStreamRequest{TRequest, TStream}"/> 
        /// containing the request and stream of the grpc method.
        /// </param>
        /// <param name="context">The <see cref="ServerCallContext"/>.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task RunAsync(IServerStreamRequest<TRequest, TStreamResponse> streamRequest, ServerCallContext context);
    }
}
