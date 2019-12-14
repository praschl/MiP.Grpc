using System;
using System.Threading.Tasks;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayHelloQuery : IQuery<HelloRequest, HelloReply>
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
