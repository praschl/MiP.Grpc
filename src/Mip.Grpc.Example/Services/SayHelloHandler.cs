using System;
using System.Threading.Tasks;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    // TODO: add an example with authorization
    public class SayHelloHandler : IHandler<HelloRequest, HelloReply>
    {
        public Task<HelloReply> RunAsync(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine(context.Host);

            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
