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

        public async Task<TResponse> Dispatch<TRequest, TResponse, THandler>(TRequest request, ServerCallContext context, string methodName)
            where THandler : IHandler<TRequest, TResponse>
        {
            var query = _serviceProvider.GetRequiredService<THandler>();

            return await query.RunAsync(request, context).ConfigureAwait(false);
        }
    }
}
