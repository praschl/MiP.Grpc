using System;
using Microsoft.AspNetCore.Routing;
using MiP.Grpc;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Adds extension methods to the <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    public static class GrpcDispatcherCompilerEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Creates an implementation of <typeparamref name="TServiceBase"/> and maps the implementation so it can be called.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to map the implementation of the service.</param>
        /// <param name="serviceProvider">The instance of the root <see cref="IServiceProvider"/>, required to get the registered services.</param>
        /// <typeparam name="TServiceBase">The base type of the service, Greeter.GreeterBase would be an example for the Greeter service that comes with the template.</typeparam>
        /// <param name="configureService">An action used to further configure the generated service class and mapping.</param>
        /// <returns><see cref="GrpcServiceEndpointConventionBuilder"/> that can be used to customize the mapping.</returns>
        public static GrpcServiceEndpointConventionBuilder CompileAndMapGrpcServiceDispatcher<TServiceBase>(this IEndpointRouteBuilder builder, IServiceProvider serviceProvider, Action<IMapGrpcServiceConfigurationBuilder> configureService = null)
        {
            return builder.CompileAndMapGrpcServiceDispatcher(serviceProvider, typeof(TServiceBase), configureService);
        }

        /// <summary>
        /// Creates an implementation of <paramref name="serviceBaseType"/> and maps the implementation so it can be called.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to map the implementation of the service.</param>
        /// <param name="serviceProvider">The instance of the root <see cref="IServiceProvider"/>, required to get the registered services.</param>
        /// <param name="serviceBaseType">The base type of the service, Greeter.GreeterBase would be an example for the Greeter service that comes with the template.</param>
        /// <param name="configureService">An action used to further configure the generated service class and mapping.</param>
        /// <returns><see cref="GrpcServiceEndpointConventionBuilder"/> that can be used to customize the mapping.</returns>
        public static GrpcServiceEndpointConventionBuilder CompileAndMapGrpcServiceDispatcher(this IEndpointRouteBuilder builder, IServiceProvider serviceProvider, Type serviceBaseType, Action<IMapGrpcServiceConfigurationBuilder> configureService = null)
        {
            if (serviceBaseType == null)
                throw new ArgumentNullException(nameof(serviceBaseType));

            MapGrpcServiceConfiguration configuration = new MapGrpcServiceConfiguration();
            configureService?.Invoke(configuration);

            var handlerStore = serviceProvider.GetService<IHandlerStore>();

            // compile a dispatchertype
            DispatcherCompiler compiler = new DispatcherCompiler(handlerStore, configuration);
            var dispatcherType = compiler.CompileDispatcher(serviceBaseType);

            return MapGrpcService(builder, dispatcherType);
        }

        private static GrpcServiceEndpointConventionBuilder MapGrpcService(IEndpointRouteBuilder builder, Type dispatcherType)
        {
            // now we need to call a generic method without having a generic type parameter
            var extensionsType = typeof(GrpcEndpointRouteBuilderExtensions);

            // so we get the method, add the type parameter with reflection and invoke it.
            var method = extensionsType.GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));
            var generic = method.MakeGenericMethod(dispatcherType);
            var endpointConventionBuilder = (GrpcServiceEndpointConventionBuilder)generic.Invoke(null, new[] { builder });

            return endpointConventionBuilder;
        }
    }
}
