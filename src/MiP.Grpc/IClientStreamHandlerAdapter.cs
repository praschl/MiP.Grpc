using Grpc.Core;

namespace MiP.Grpc
{
    /// <summary>
    /// This is an adapter interface that helps the generated code to treat
    /// <see cref="IClientStreamHandler{TRequest, TStream}"/> 
    /// just like a <see cref="IHandler{TRequest, TResponse}"/> 
    /// with the TResponse being a <see cref="IAsyncStreamReader{T}"/>.
    /// </summary>
    /// <typeparam name="TStreamRequest">Type of the objects streamed from the client to the server.</typeparam>
    /// <typeparam name="TResponse">Type of the response from the server once streaming is finished.</typeparam>
    /// <typeparam name="TClientStreamHandler">Type of the handler thats being adapted.</typeparam>
    public interface IClientStreamHandlerAdapter<TStreamRequest, TResponse, TClientStreamHandler>
    : IHandler<IAsyncStreamReader<TStreamRequest>, TResponse>
        where TClientStreamHandler : IClientStreamHandler<TStreamRequest, TResponse>
    {
    }
}
