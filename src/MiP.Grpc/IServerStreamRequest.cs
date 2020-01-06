using Grpc.Core;

namespace MiP.Grpc
{
    /// <summary>
    /// Used as wrapper for the request and stream parameter of a server streaming grpc method.
    /// </summary>
    /// <typeparam name="TRequest">Type of the request object that initiates the stream.</typeparam>
    /// <typeparam name="TStreamResponse">Type of the object that can be streamed by the server.</typeparam>
    public interface IServerStreamRequest<TRequest, TStreamResponse>
    {
        /// <summary>
        /// The request that initiates the stream.
        /// </summary>
        TRequest Request { get; }

        /// <summary>
        /// A stream writer used to stream objects to the client.
        /// </summary>
        IServerStreamWriter<TStreamResponse> Stream { get; }
    }
}
