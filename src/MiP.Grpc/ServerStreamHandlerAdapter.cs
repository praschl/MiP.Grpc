using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    internal class ServerStreamHandlerAdapter<TRequest, TStreamResponse, TServerStreamHandler>
        : IServerStreamHandlerAdapter<TRequest, TStreamResponse, TServerStreamHandler>
        where TServerStreamHandler : IServerStreamHandler<TRequest, TStreamResponse>
    {
        private readonly TServerStreamHandler _serverStreamHandler;

        public ServerStreamHandlerAdapter(TServerStreamHandler serverStreamHandler)
        {
            _serverStreamHandler = serverStreamHandler;
        }

        public async Task<Empty> RunAsync(IServerStreamRequest<TRequest, TStreamResponse> streamRequest, ServerCallContext context)
        {
            await _serverStreamHandler.RunAsync(streamRequest, context).ConfigureAwait(false);
            return new Empty();
        }
    }
}
