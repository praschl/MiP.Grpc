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

            var dispatcherMap = serviceProvider.GetService<IDispatcherMapBuilder>();

            DispatcherCompiler compiler = new DispatcherCompiler(dispatcherMap);

            var dispatcherType = compiler.CompileDispatcher(serviceBaseType);

            var extensionsType = typeof(GrpcEndpointRouteBuilderExtensions);

            var method = extensionsType.GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));
            var generic = method.MakeGenericMethod(dispatcherType);
            var invocationResult = generic.Invoke(null, new[] { builder });

            return (GrpcServiceEndpointConventionBuilder)invocationResult;
        }
    }
}
