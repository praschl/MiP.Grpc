using System;
using Microsoft.AspNetCore.Routing;
using MiP.Grpc;

namespace Microsoft.AspNetCore.Builder
{
    public static class GrpcDispatcherCompilerEndpointRouteBuilderExtensions
    {
        public static GrpcServiceEndpointConventionBuilder CompileAndMapGrpcServiceDispatcher(this IEndpointRouteBuilder builder, Type serviceBaseType)
        {
            DispatcherCompiler compiler = new DispatcherCompiler();

            var dispatcherType = compiler.CompileDispatcher(serviceBaseType);

            var extensionsType = typeof(GrpcEndpointRouteBuilderExtensions);

            var method = extensionsType.GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));
            var generic = method.MakeGenericMethod(dispatcherType);
            var invocationResult = generic.Invoke(null, new[] { builder });

            return (GrpcServiceEndpointConventionBuilder)invocationResult;
        }
    }
}
