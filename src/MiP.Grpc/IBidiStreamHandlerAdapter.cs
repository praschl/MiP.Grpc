using Grpc.Core;
using Protobuf = Google.Protobuf.WellKnownTypes;

namespace MiP.Grpc
{
    /// <summary>
    /// This is an adapter interface that helps the generated code to treat 
    /// <see cref="IBidiStreamHandler{TRequest, TStream}"/> 
    /// just like a <see cref="IHandler{TRequest, TResponse}"/> 
    /// with TRequest being a <see cref="IAsyncStreamReader{T}"/>
    /// and TResponse being a <see cref="IServerStreamWriter{T}"/>.
    /// </summary>
    /// <typeparam name="TStreamRequest">Type of the objects streamed from the client to the server.</typeparam>
    /// <typeparam name="TStreamResponse">Type of the objects streamed from the server to the client.</typeparam>
    /// <typeparam name="TBidiStreamHandler">Type of the handler thats being adapted.</typeparam>
    public interface IBidiStreamHandlerAdapter<TStreamRequest, TStreamResponse, TBidiStreamHandler>
        : IHandler<IBidiStreamRequest<TStreamRequest, TStreamResponse>, Protobuf.Empty>
        where TBidiStreamHandler : IBidiStreamHandler<TStreamRequest, TStreamResponse>
    {
    }
}
