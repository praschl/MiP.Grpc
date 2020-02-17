using Grpc.Core;

namespace MiP.Grpc
{
    /// <summary>
    /// Used as wrapper for the request and stream parameter of a server streaming grpc method.
    /// </summary>
    /// <typeparam name="TStreamRequest">Type of the objects streamed from the server to the client.</typeparam>
    /// <typeparam name="TStreamResponse">Type of the objects streamed from the client to the server.</typeparam>
    public interface IBidiStreamRequest<TStreamRequest, TStreamResponse>
    {
        /// <summary>
        /// A stream reader used to read the objects that are streamed from the client.
        /// </summary>
        IAsyncStreamReader<TStreamRequest> RequestStream { get; }

        /// <summary>
        /// A stream writer used to stream objects to the client.
        /// </summary>
        IServerStreamWriter<TStreamResponse> ResponseStream { get; }
    }
}
