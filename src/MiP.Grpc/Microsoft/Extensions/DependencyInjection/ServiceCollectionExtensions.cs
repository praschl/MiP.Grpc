using MiP.Grpc;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Adds extention methods to the <see cref="IServiceCollection"/> that help with configuring and registering handlers for the grpc service dispatcher.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="IDispatcher"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="buildDispatcherMap">An action that can be used to register all implementations of <see cref="IHandler{TRequest, TResponse}"/> 
        /// of an assembly, or register handlers manually.</param>
        /// <returns>The <paramref name="services"/>.</returns>
        public static IServiceCollection AddDispatchedGrpcHandlers(this IServiceCollection services,
            Action<IDispatcherMapBuilder> buildDispatcherMap = null
            )
        {
            services.AddTransient<IDispatcher, Dispatcher>();
            services.AddTransient(typeof(ICommandHandlerAdapter<,>), typeof(CommandHandlerAdapter<,>));
            services.AddTransient(typeof(IServerStreamHandlerAdapter<,,>), typeof(ServerStreamHandlerAdapter<,,>));
            services.AddTransient(typeof(IClientStreamHandlerAdapter<,,>), typeof(ClientStreamHandlerAdapter<,,>));
            services.AddTransient(typeof(IBidiStreamHandlerAdapter<,,>), typeof(BidiStreamHandlerAdapter<,,>));

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