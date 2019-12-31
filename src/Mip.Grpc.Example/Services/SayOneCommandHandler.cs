using System;
using System.Threading.Tasks;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayOneCommandHandler : ICommandHandler<OneCommand>
    {
        public Task RunAsync(OneCommand command, ServerCallContext context)
        {
            Console.WriteLine("OneCommand called: " + command.One);

            return Task.CompletedTask;
        }
    }
}
