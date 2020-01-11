using System.Threading.Tasks;
using Proto = Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MiP.Grpc;
using System;

namespace Mip.Grpc.Example.ForGreeter
{
    [Handles(ServiceBase = typeof(Greeter.GreeterBase))]
    public class DuplicateHandler : IHandler<Proto.Empty, Proto.Empty>
    {
        public Task<Proto.Empty> RunAsync(Proto.Empty request, ServerCallContext context)
        {
            Console.WriteLine("This is handler ForGreeter");

            return Task.FromResult(new Proto.Empty());
        }
    }
}

namespace Mip.Grpc.Example.ForCalc
{
    public class DuplicateHandler : IHandler<Proto.Empty, Proto.Empty>
    {
        [Handles(ServiceBase = typeof(Calc.Calculator.CalculatorBase))]
        public Task<Proto.Empty> RunAsync(Proto.Empty request, ServerCallContext context)
        {
            Console.WriteLine("This is handler ForCalc");

            return Task.FromResult(new Proto.Empty());
        }
    }
}
