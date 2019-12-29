using Grpc.Core;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Protobuf = Google.Protobuf.WellKnownTypes;

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
        {
            var service = _serviceProvider.GetRequiredService<THandler>();

            if (service is IHandler<TRequest, TResponse> handler)
                return await handler.RunAsync(request, context).ConfigureAwait(false);

            if (typeof(TResponse) == typeof(Protobuf.Empty) && service is ICommandHandler<TRequest> commandHandler)
            {
                await commandHandler.RunAsync(request, context).ConfigureAwait(false);
                return default;
            }

            throw new InvalidOperationException($"Handler [{typeof(THandler).FullName}] must implement either [IHandler<{typeof(TRequest).FullName},{typeof(TResponse).FullName}>] or [ICommandHandler<{typeof(TRequest).FullName}>]");
        }
    }
}
