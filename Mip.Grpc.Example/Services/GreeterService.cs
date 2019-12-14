using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace Mip.Grpc.Example
{
    public class GreeterService : Greeter.GreeterBase
    {
        public GreeterService(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Got SP: " + serviceProvider.GetHashCode());
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<HowdyReply> SayHowdy(HowdyRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HowdyReply
            {
                Message = "Howdy" + request.Name,
                Number = request.Number + 1
            });
        }
    }
}
