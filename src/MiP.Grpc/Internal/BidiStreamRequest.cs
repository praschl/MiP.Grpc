using Grpc.Core;
using System;

namespace MiP.Grpc.Internal
{
    /// <summary>
    /// For internal use only.
    /// </summary>
    [Obsolete("For internal use only.")]
    public class BidiStreamRequest<TStreamRequest, TStreamResponse> : IBidiStreamRequest<TStreamRequest, TStreamResponse>
    {
        /// <summary>
        /// For internal use only.
        /// </summary>
        public BidiStreamRequest(IAsyncStreamReader<TStreamRequest> requestStream, IServerStreamWriter<TStreamResponse> responseStream)
        {
            RequestStream = requestStream;
            ResponseStream = responseStream;
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        public IAsyncStreamReader<TStreamRequest> RequestStream { get; }

        /// <summary>
        /// For internal use only.
        /// </summary>
        public IServerStreamWriter<TStreamResponse> ResponseStream { get; }
    }
}
