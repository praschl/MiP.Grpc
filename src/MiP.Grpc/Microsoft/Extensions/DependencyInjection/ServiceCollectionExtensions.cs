﻿using MiP.Grpc;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDispatchedGrpcHandlers(this IServiceCollection services, Assembly[] fromAssemblies = null)
        {
            services.AddTransient<IDispatcher, Dispatcher>();

            if (fromAssemblies != null)
            {
                foreach (var assembly in fromAssemblies)
                {
                    AddAssemblyQueries(assembly, services);
                }
            }

            return services;
        }

        private static void AddAssemblyQueries(Assembly assembly, IServiceCollection services)
        {
            var typesToRegister = GetTypeInfos(assembly.GetTypes());

            foreach (var typeTuple in typesToRegister)
            {
                services.AddTransient(typeTuple.service, typeTuple.implementation);
            }
        }

        private static IEnumerable<(Type service, Type implementation)> GetTypeInfos(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                foreach (var iface in interfaces)
                {
                    if (!iface.IsGenericType)
                        continue;

                    if (iface.GetGenericTypeDefinition() != typeof(IHandler<,>))
                        continue;

                    yield return (iface, type);
                }
            }
        }
    }
}