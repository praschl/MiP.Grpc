using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    public interface IHandler<TRequest, TResponse>
    {
        Task<TResponse> RunAsync(TRequest request, ServerCallContext context);
    }
}
