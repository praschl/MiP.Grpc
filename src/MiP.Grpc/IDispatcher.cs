using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    public interface IDispatcher
    {
        Task<TResponse> Dispatch<TRequest, TResponse, THandler>(TRequest request, ServerCallContext context)
            where THandler : IHandler<TRequest, TResponse>;
    }
}

