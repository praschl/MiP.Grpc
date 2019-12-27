using System;
using Microsoft.AspNetCore.Routing;
using MiP.Grpc;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class GrpcDispatcherCompilerEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Creates an implementation of <paramref name="serviceBaseType"/> and maps the implementation so it can be called.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> used to map the implementation of the service.</param>
        /// <param name="serviceBaseType">The base type of the service, Greeter.GreeterBase would be an example for the Greeter service that comes with the template.</param>
        /// <returns><see cref="GrpcServiceEndpointConventionBuilder"/> that can be used to customize the mapping.</returns>
        public static GrpcServiceEndpointConventionBuilder CompileAndMapGrpcServiceDispatcher(this IEndpointRouteBuilder builder, IServiceProvider serviceProvider, Type serviceBaseType)
        {
            if (serviceBaseType == null)
                throw new ArgumentNullException(nameof(serviceBaseType));

            // TODO: add possibility to add AuthorizationAttribute to all methods of the service of parameter serviceBaseType

            var handlerStore = serviceProvider.GetService<IHandlerStore>();

            // compile a dispatchertype
            DispatcherCompiler compiler = new DispatcherCompiler(handlerStore);
            var dispatcherType = compiler.CompileDispatcher(serviceBaseType);

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
