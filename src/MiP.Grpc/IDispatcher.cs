using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    public interface IDispatcher
    {
        Task<TResponse> Dispatch<TRequest, TResponse>(TRequest request, ServerCallContext context);
    }
}

