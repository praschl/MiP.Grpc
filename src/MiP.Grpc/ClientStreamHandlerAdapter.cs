using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    internal class ClientStreamHandlerAdapter<TStreamRequest, TResponse, TClientStreamHandler>
        : IClientStreamHandlerAdapter<TStreamRequest, TResponse, TClientStreamHandler>
        where TClientStreamHandler : IClientStreamHandler<TStreamRequest, TResponse>
    {
        private readonly TClientStreamHandler _clientStreamHandler;

        public ClientStreamHandlerAdapter(TClientStreamHandler clientStreamHandler)
        {
            _clientStreamHandler = clientStreamHandler;
        }

        public async Task<TResponse> RunAsync(IAsyncStreamReader<TStreamRequest> request, ServerCallContext context)
        {
            return await _clientStreamHandler.RunAsync(request, context).ConfigureAwait(false);
        }
    }
}
