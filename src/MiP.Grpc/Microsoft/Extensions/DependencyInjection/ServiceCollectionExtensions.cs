using MiP.Grpc;
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
            Assembly[] fromAssemblies = null,
            Action<IDispatcherMapBuilder> buildDispatcherMap = null
            )
        {
            services.AddTransient<IDispatcher, Dispatcher>();

            DispatcherMapBuilder mapBuilder = new DispatcherMapBuilder();
            services.AddSingleton<IDispatcherMapBuilder>(mapBuilder);

            // first take all automatic
            if (fromAssemblies != null)
            {
                foreach (var assembly in fromAssemblies)
                {
                    AddHandlersFromAssembly(assembly, mapBuilder);
                }
            }

            // then the manually added to overwrite, if needed
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

        private static void AddHandlersFromAssembly(Assembly assembly, DispatcherMapBuilder mapBuilder)
        {
            var handlerInfos = GetTypeInfos(assembly.GetTypes(), mapBuilder);

            foreach (var handlerInfo in handlerInfos)
            {
                mapBuilder.Add(handlerInfo, null);
            }
        }

        private static IEnumerable<HandlerInfo> GetTypeInfos(IEnumerable<Type> types, DispatcherMapBuilder mapBuilder)
        {
            var handlerInfos = types.Select(mapBuilder.GetIHandlers)
                .Where(hi => hi.ServiceTypes.Count > 0);

            return handlerInfos;
        }
    }
}