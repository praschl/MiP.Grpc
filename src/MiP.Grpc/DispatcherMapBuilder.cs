using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MiP.Grpc
{
    internal class DispatcherMapBuilder : IDispatcherMapBuilder, IHandlerStore
    {
        private readonly List<DispatcherMap> _dispatcherMaps = new List<DispatcherMap>();

        public IReadOnlyList<DispatcherMap> DispatcherMaps
        {
            get => _dispatcherMaps;
        }

        public IDispatcherMapBuilder Add<THandler>(string name)
        {
            AddInternal(typeof(THandler), name);

            return this;
        }

        public IDispatcherMapBuilder Add(Type handlerType, string name)
        {
            AddInternal(handlerType, name);

            return this;
        }

        public IDispatcherMapBuilder Add(Assembly assembly)
        {
            AddHandlersFromAssembly(assembly);

            return this;
        }

        public Type FindHandler(string methodName, Type parameterType, Type returnType)
        {
            var requestedServiceType = typeof(IHandler<,>).MakeGenericType(parameterType, returnType);

            var map = _dispatcherMaps.FirstOrDefault(m =>
                m.MethodName == methodName
                &&
                m.ServiceType == requestedServiceType);

            return map?.HandlerType;
        }

        private void AddHandlersFromAssembly(Assembly assembly)
        {
            var handlerInfos = GetTypeInfos(assembly.GetTypes());

            foreach (var handlerInfo in handlerInfos)
            {
                Add(handlerInfo, null);
            }
        }

        private static IEnumerable<HandlerInfo> GetTypeInfos(IEnumerable<Type> types)
        {
            var handlerInfos = types.Select(DispatcherMapBuilder.GetIHandlers)
                .Where(hi => hi.ServiceTypes.Count > 0);

            return handlerInfos;
        }

        private void Add(HandlerInfo handlerInfo, string name)
        {
            name ??= handlerInfo.GetPreferredName();

            foreach (var service in handlerInfo.ServiceTypes)
            {
                Type implementation = handlerInfo.Implementation;

                var existing = _dispatcherMaps.Where(h => h.MethodName == name && h.ServiceType == service).ToArray();
                if (existing.Length > 0)
                {
                    Debug.WriteLine($"There is already a combination of Name '{name}' and [{service}].");
                    foreach (var item in existing)
                    {
                        // unregister this one, so we dont have duplicates
                        _dispatcherMaps.Remove(item);
                    }
                }

                var newMap = new DispatcherMap(name, implementation, service);
                _dispatcherMaps.Add(newMap);
            }
        }

        private void AddInternal(Type handlerType, string name)
        {
            var handlerInfo = GetIHandlers(handlerType);

            Add(handlerInfo, name);
        }

        private static HandlerInfo GetIHandlers(Type type)
        {
            var interfaces = type.GetInterfaces();

            var services = interfaces.Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IHandler<,>)
                );

            return new HandlerInfo(type, services.ToArray());
        }
    }
}