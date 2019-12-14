using System;
using System.Threading.Tasks;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayHowdyQuery : IQuery<HowdyRequest, HowdyReply>
    {
        private Guid _guid = Guid.NewGuid();
        
        public Task<HowdyReply> RunAsync(HowdyRequest request)
        {
            return Task.FromResult(new HowdyReply
            {
                Message = "NEW Howdy" + request.Name + _guid,
                Number = request.Number + 1
            });
        }
    }
}
