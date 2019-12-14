using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    public interface IQuery<TRequest, TResponse>
    {
        Task<TResponse> RunAsync(TRequest request, ServerCallContext context);
    }
}
