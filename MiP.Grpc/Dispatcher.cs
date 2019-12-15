using Grpc.Core;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MiP.Grpc
{
    internal class Dispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public Dispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Dispatch<TRequest, TResponse>(TRequest request, ServerCallContext context)
        {
            var query = _serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>();

            return await query.RunAsync(request, context).ConfigureAwait(false);
        }
    }
}

