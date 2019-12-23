﻿using MiP.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        /// <param name="fromAssemblies">Assemblies that should be scanned for types which implement <see cref="IHandler{TRequest, TResponse}"/>.</param>
        /// <returns>The <paramref name="services"/>.</returns>
        public static IServiceCollection AddDispatchedGrpcHandlers(this IServiceCollection services,
            Action<IDispatcherMapBuilder> buildDispatcherMap = null
            )
        {
            services.AddTransient<IDispatcher, Dispatcher>();

            DispatcherMapBuilder mapBuilder = new DispatcherMapBuilder();
            services.AddSingleton<IDispatcherMapBuilder>(mapBuilder);

            buildDispatcherMap?.Invoke(mapBuilder);

            // and register them
            foreach (var group in mapBuilder.DispatcherMaps.GroupBy(i => i.HandlerType))
            {
                services.AddTransient(group.Key);
                foreach (var item in group)
                {
                    services.AddTransient(item.ServiceType, group.Key);
                }
            }

            return services;
        }
    }
}