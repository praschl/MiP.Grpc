using System;
using System.Threading.Tasks;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayOneCommandHandler : ICommandHandler<OneRequest>
    {
        public Task RunAsync(OneRequest request, ServerCallContext context)
        {
            Console.WriteLine("OneCommand called: " + request.One);

            return Task.CompletedTask;
        }
    }
}
