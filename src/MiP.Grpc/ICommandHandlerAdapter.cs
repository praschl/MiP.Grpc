using Google.Protobuf.WellKnownTypes;

namespace MiP.Grpc
{
    /// <summary>
    /// This is an adapter interface that helps the generated code to treat 
    /// <see cref="ICommandHandler{TRequest}"/> just like a 
    /// <see cref="IHandler{TRequest, TResponse}"/> with TResponse being void.
    /// This interface is public for the code generation to work properly, and should not be used directly.
    /// </summary>
    /// <typeparam name="TCommand">Request type of the command to be adapted.</typeparam>
    /// <typeparam name="TCommandHandler">Type of the handler thats being adapted.</typeparam>
    public interface ICommandHandlerAdapter<TCommand, TCommandHandler> : IHandler<TCommand, Empty>
        where TCommandHandler : ICommandHandler<TCommand>
    {
    }
}
