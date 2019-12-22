using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayNothingHandler : IHandler<Empty, Empty>
    {
        public Task<Empty> RunAsync(Empty request, ServerCallContext context)
        {
            Console.WriteLine("SAY NOTHING called: " + context.Host);

            return Task.FromResult(new Empty());
        }
    }
}
