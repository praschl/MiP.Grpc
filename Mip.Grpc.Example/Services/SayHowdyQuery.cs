using System;
using System.Threading.Tasks;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayHowdyQuery : IQuery<HowdyRequest, HowdyReply>
    {
        private Guid _guid = Guid.NewGuid();

        public Task<HowdyReply> RunAsync(HowdyRequest request, ServerCallContext context)
        {
            Console.WriteLine(context.Host);

            return Task.FromResult(new HowdyReply
            {
                Message = "NEW Howdy" + request.Name + _guid,
                Number = request.Number + 1
            });
        }
    }
}
