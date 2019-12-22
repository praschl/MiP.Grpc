using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    [Handles(nameof(Greeter.GreeterBase.SayNothing))]
    public class AlternativeSayNothingHandler : IHandler<Empty, Empty>
    {
        public Task<Empty> RunAsync(Empty request, ServerCallContext context)
        {
            Console.WriteLine("ALTERNATIVE SAY NOTHING called: " + context.Host);

            return Task.FromResult(new Empty());
        }
    }
}
