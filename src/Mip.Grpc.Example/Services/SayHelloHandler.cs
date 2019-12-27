using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    // TODO: add an example with authorization
    public class SayHelloHandler : IHandler<HelloRequest, HelloReply>
    {
        private Guid _guid = Guid.NewGuid();

        public Task<HelloReply> RunAsync(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine(context.Host);

            return Task.FromResult(new HelloReply
            {
                Message = "NEW Hello " + request.Name + _guid
            });
        }
    }
}
