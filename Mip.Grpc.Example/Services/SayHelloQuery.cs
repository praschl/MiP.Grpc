using System.Threading.Tasks;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayHelloQuery : IQuery<HelloRequest, HelloReply>
    {
        public Task<HelloReply> RunAsync(HelloRequest request)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "NEW Hello " + request.Name
            });
        }
    }
}
