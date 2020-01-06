using System;
using System.Threading.Tasks;
using Grpc.Core;
using MiP.Grpc;

namespace Mip.Grpc.Example
{
    public class SayHowdyHandler : IHandler<HowdyRequest, HowdyReply>
    {
        // explicit implementations are found, too
        Task<HowdyReply> IHandler<HowdyRequest, HowdyReply>.RunAsync(HowdyRequest request, ServerCallContext context)
        {
            Console.WriteLine(context.Host);

            return Task.FromResult(new HowdyReply
            {
                Message = "Howdy" + request.Name,
                Number = request.Number + 1
            });
        }
    }
}
