using System.Threading.Tasks;
using Grpc.Core;
using Mip.Grpc.Example.Calc;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class AddHandler : IHandler<AddRequest, AddResponse>
    {
        public Task<AddResponse> RunAsync(AddRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AddResponse { Res = request.A + request.B });
        }
    }
}
