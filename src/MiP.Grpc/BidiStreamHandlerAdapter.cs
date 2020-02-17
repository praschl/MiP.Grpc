using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    internal class BidiStreamHandlerAdapter<TStreamRequest, TStreamResponse, TBidiStreamHandler>
        : IBidiStreamHandlerAdapter<TStreamRequest, TStreamResponse, TBidiStreamHandler>
        where TBidiStreamHandler : IBidiStreamHandler<TStreamRequest, TStreamResponse>
    {
        private readonly TBidiStreamHandler _bidiStreamHandler;

        public BidiStreamHandlerAdapter(TBidiStreamHandler bidiStreamHandler)
        {
            _bidiStreamHandler = bidiStreamHandler;
        }

        public async Task<Empty> RunAsync(IBidiStreamRequest<TStreamRequest, TStreamResponse> streamRequest, ServerCallContext context)
        {
            await _bidiStreamHandler.RunAsync(streamRequest, context).ConfigureAwait(false);
            return new Empty();
        }
    }
}
