using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    /// <summary>
    /// Implemented to make a class handle a specific service method that 
    /// streams objects bidirectionally between client and service.
    /// </summary>
    /// <typeparam name="TStreamRequest">Type of the objects streamed from the client to the server.</typeparam>
    /// <typeparam name="TStreamResponse">Type of the objects streamed from the server to the client.</typeparam>
    public interface IBidiStreamHandler<TStreamRequest, TStreamResponse>
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="streamRequest">A stream request containing both the server and the client streams.</param>
        /// <param name="context">The <see cref="ServerCallContext"/>.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task RunAsync(IBidiStreamRequest<TStreamRequest, TStreamResponse> streamRequest, ServerCallContext context);
    }
}
