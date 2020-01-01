using Grpc.Core;
using System.Threading.Tasks;
using Protobuf = Google.Protobuf.WellKnownTypes;

namespace MiP.Grpc.Internal
{
    internal class CommandHandlerAdapter<TCommand, TCommandHandler> : ICommandHandlerAdapter<TCommand, TCommandHandler>
        where TCommandHandler : ICommandHandler<TCommand>
    {
        private readonly TCommandHandler _commandHandler;

        public CommandHandlerAdapter(TCommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public async Task<Protobuf.Empty> RunAsync(TCommand command, ServerCallContext context)
        {
            await _commandHandler.RunAsync(command, context).ConfigureAwait(false);
            return new Protobuf.Empty();
        }
    }
}
