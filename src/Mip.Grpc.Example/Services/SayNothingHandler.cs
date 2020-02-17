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
            throw new InvalidOperationException("This handler should not be called because overridden by AlternativeSayNothingHandler!");
        }
    }
}
