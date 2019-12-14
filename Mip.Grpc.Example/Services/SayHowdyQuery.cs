using System.Threading.Tasks;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayHowdyQuery : IQuery<HowdyRequest, HowdyReply>
    {
        public Task<HowdyReply> RunAsync(HowdyRequest request)
        {
            return Task.FromResult(new HowdyReply
            {
                Message = "NEW Howdy" + request.Name,
                Number = request.Number + 1
            });
        }
    }
}
