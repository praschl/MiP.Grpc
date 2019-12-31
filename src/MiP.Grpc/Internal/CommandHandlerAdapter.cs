using Grpc.Core;
using System.Threading.Tasks;
using Protobuf = Google.Protobuf.WellKnownTypes;

namespace MiP.Grpc.Internal
{
    /// <summary>
    /// This is an adapter that helps the generated code to treat <see cref="ICommandHandler{TRequest}"/> just like a <see cref="IHandler{TRequest, TResponse}"/> with TResponse being void.
    /// This class is public for the code generation to work properly, and should not be used directly.
    /// </summary>
    /// <typeparam name="TCommand">Request type of the command to be adapted.</typeparam>
    /// <typeparam name="TCommandHandler">Type of the handler thats being adapted.</typeparam>
    public class CommandHandlerAdapter<TCommand, TCommandHandler> : IHandler<TCommand, Protobuf.Empty>
        where TCommandHandler : ICommandHandler<TCommand>
    {
        private readonly TCommandHandler _commandHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandlerAdapter{TRequest, TCommandHandler}"/>.
        /// </summary>
        /// <param name="commandHandler">The handler being adapted.</param>
        public CommandHandlerAdapter(TCommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        /// <summary>
        /// Runs the command, passes the <paramref name="command" /> and <paramref name="context" /> and returns a new instance of <see cref="Protobuf.Empty"/>.
        /// </summary>
        /// <param name="command">The request parameter.</param>
        /// <param name="context">The <see cref="ServerCallContext" />.</param>
        /// <returns>A <see cref="Task" /> that will contain the <see cref="Protobuf.Empty"/> result of the handler.</returns>
        public async Task<Protobuf.Empty> RunAsync(TCommand command, ServerCallContext context)
        {
            await _commandHandler.RunAsync(command, context).ConfigureAwait(false);
            return new Protobuf.Empty();
        }
    }
}
