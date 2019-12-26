using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MiP.Grpc
{
    internal class DispatcherMapBuilder : IDispatcherMapBuilder, IHandlerStore
    {
        private readonly Dictionary<DispatcherMapKey, DispatcherMap> _dispatcherMaps = new Dictionary<DispatcherMapKey, DispatcherMap>();

        public IEnumerable<Type> GetHandlers()
        {
            return _dispatcherMaps.Values.GroupBy(m => m.HandlerType).Select(g => g.Key);
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

            _dispatcherMaps.TryGetValue(new DispatcherMapKey(methodName, requestedServiceType), out var map);

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
            var handlerInfos = types.Select(GetIHandlers)
                .Where(hi => hi.ServiceTypes.Count > 0);

            return handlerInfos;
        }

        private void Add(HandlerInfo handlerInfo, string name)
        {
            if (string.IsNullOrEmpty(name))
                name = handlerInfo.GetPreferredName();

            foreach (var service in handlerInfo.ServiceTypes)
            {
                Type implementation = handlerInfo.Implementation;

                var key = new DispatcherMapKey(name, service);
                if (_dispatcherMaps.ContainsKey(key))
                {
                    Debug.WriteLine($"There is already a handler for method '{name}' and [{service}]. It will be removed and [{implementation.FullName}] will handle that.");
                    _dispatcherMaps.Remove(key);
                }

                var newMap = new DispatcherMap(key, implementation);
                _dispatcherMaps.Add(key, newMap);
            }
        }

        private void AddInternal(Type handlerType, string name)
        {
            var handlerInfo = GetIHandlers(handlerType);

            if (handlerInfo.ServiceTypes.Count == 0)
                throw new InvalidOperationException($"Type [{handlerType.FullName}] has no implementation of [{typeof(IHandler<,>).FullName}].");

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