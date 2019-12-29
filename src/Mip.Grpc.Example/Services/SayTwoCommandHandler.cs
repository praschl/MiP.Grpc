using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayTwoCommandHandler : ICommandHandler<Empty>
    {
        public Task RunAsync(Empty request, ServerCallContext context)
        {
            Console.WriteLine("TwoCommand called: " + context.Host);

            return Task.CompletedTask;
        }
    }
}
