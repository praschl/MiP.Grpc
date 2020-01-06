using Grpc.Core;
using Protobuf = Google.Protobuf.WellKnownTypes;

namespace MiP.Grpc
{
    /// <summary>
    /// This is an adapter interface that helps the generated code to treat 
    /// <see cref="IServerStreamHandler{TRequest, TStream}"/> 
    /// just like a <see cref="IHandler{TRequest, TResponse}"/> 
    /// with TResponse being a <see cref="IServerStreamWriter{T}"/>.
    /// </summary>
    /// <typeparam name="TRequest">Type of request that initiates the stream.</typeparam>
    /// <typeparam name="TStreamResponse">Type of the objects streamed by from the the server to the client.</typeparam>
    /// <typeparam name="TServerStreamHandler">Type of the handler thats being adapted.</typeparam>
    public interface IServerStreamHandlerAdapter<TRequest, TStreamResponse, TServerStreamHandler>
        : IHandler<IServerStreamRequest<TRequest, TStreamResponse>, Protobuf.Empty>
        where TServerStreamHandler : IServerStreamHandler<TRequest, TStreamResponse>
    {
    }
}
