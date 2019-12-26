using MiP.Grpc;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="IDispatcher"/>.
        /// Scans all <paramref name="fromAssemblies"/> for types that implement <see cref="IHandler{TRequest, TResponse}"/>
        /// and registers them as handlers.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The <paramref name="services"/>.</returns>
        public static IServiceCollection AddDispatchedGrpcHandlers(this IServiceCollection services,
            Action<IDispatcherMapBuilder> buildDispatcherMap = null
            )
        {
            services.AddTransient<IDispatcher, Dispatcher>();

            DispatcherMapBuilder mapBuilder = new DispatcherMapBuilder();
            services.AddSingleton<IHandlerStore>(mapBuilder);

            buildDispatcherMap?.Invoke(mapBuilder);

            // and register them
            foreach (var handler in mapBuilder.GetHandlers())
            {
                services.AddTransient(handler);
            }

            return services;
        }
    }
}