using Grpc.Core;
using System;

namespace MiP.Grpc.Internal
{
    /// <summary>
    /// For internal use only.
    /// </summary>
    [Obsolete("For internal use only.")]
    public class ServerStreamRequest<TRequest, TStreamResponse> : IServerStreamRequest<TRequest, TStreamResponse>
    {
        /// <summary>
        /// For internal use only.
        /// </summary>
        public ServerStreamRequest(TRequest request, IServerStreamWriter<TStreamResponse> stream)
        {
            Request = request;
            Stream = stream;
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        public TRequest Request { get; }

        /// <summary>
        /// For internal use only.
        /// </summary>
        public IServerStreamWriter<TStreamResponse> Stream { get; }
    }
}
