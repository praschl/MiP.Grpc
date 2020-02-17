using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiP.Grpc
{
    internal class DispatcherMapBuilder : IDispatcherMapBuilder, IHandlerStore
    {
        private readonly Dictionary<MapKey, Type> _maps = new Dictionary<MapKey, Type>();

        public IEnumerable<Type> GetHandlers()
        {
            return _maps.Values.Distinct();
        }

        public IDispatcherMapBuilder Add<THandler>(Type serviceBase, string methodName)
        {
            Add(typeof(THandler), serviceBase,  methodName);
            return this;
        }

        public IDispatcherMapBuilder Add(Type handlerType, Type serviceBase, string methodName)
        {
            // TODO: assert that the <serviceBase> even has a method that can be handled by handlerType or throw

            AssertHasHandlerInterface(handlerType);

            if (serviceBase == null)
                AssertHasServiceBaseSetForAllInterfaces(handlerType);

            AddInternal(handlerType, serviceBase, methodName);
            return this;
        }

        public IDispatcherMapBuilder Add(Assembly assembly, Type serviceBase)
        {
            var foundHandlers = assembly.GetTypes()
                .Where(t => t.HasHandlerInterface());

            foreach (var handler in foundHandlers)
            {
                // if set in parameter: only add handlers that have a method the servicebase offers

                if (serviceBase == null)
                    AssertHasServiceBaseSetForAllInterfaces(handler);

                AddInternal(handler, serviceBase, null);
            }

            return this;
        }

        private void AssertHasServiceBaseSetForAllInterfaces(Type handler)
        {
            // if there is a HandlesAttribute with ServiceBase set on the class, we are done, 
            // since that would be the fallback if there is no attribute on the method.
            if (handler.GetCustomAttribute<HandlesAttribute>()?.ServiceBase != null)
                return;

            var interfaces = handler.GetInterfaces()
                .Where(i => i.IsGenericType && TypeExtensions.HandlerInterfaces.Contains(i.GetGenericTypeDefinition()));

            foreach (var iface in interfaces)
            {
                var method = handler.GetInterfaceMap(iface).TargetMethods[0];
                if (method.GetCustomAttribute<HandlesAttribute>()?.ServiceBase == null)
                    throw new InvalidOperationException($"Method [{method}] on [{handler}] has no target service base.");
            }
        }

        private void AssertHasHandlerInterface(Type handler)
        {
            bool hasHandler = handler.HasHandlerInterface();
            if (!hasHandler)
                throw new InvalidOperationException($"[{handler}] does not implement one of the handler interfaces.");
        }

        private void AddInternal(Type handlerType, Type serviceBase, string methodName)
        {
            foreach (var iface in handlerType.GetHandlerInterfaces())
            {
                var currentService = serviceBase;
                string currentMethod = methodName;

                if (string.IsNullOrWhiteSpace(currentMethod))
                    currentMethod = handlerType.GetMethodName(iface);

                if (currentService == null)
                {
                    currentService = handlerType.GetServiceBase(iface);
                    // here we definitly get a servicebase, this was checked earlier
                }

                _maps[new MapKey(currentService, iface, currentMethod)] = handlerType;
            }
        }

        public HandlerInfo Find(Type serviceBase, Type handlerInterface, string methodName)
        {
            if (serviceBase == null)
                throw new ArgumentNullException(nameof(serviceBase));
            if (handlerInterface == null)
                throw new ArgumentNullException(nameof(handlerInterface));
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullException(nameof(methodName));

            // check that handlerInterface is valid
            if (!handlerInterface.IsHandlerInterface())
                throw new ArgumentException($"Argument {nameof(handlerInterface)} must be one of the handler interfaces.", nameof(handlerInterface));

            var key = new MapKey(serviceBase, handlerInterface, methodName);
            if (!_maps.TryGetValue(key, out var foundHandler))
                return null;

            var authorizeAttributes = foundHandler.GetAuthorizeAttributes(handlerInterface);

            return new HandlerInfo(foundHandler, authorizeAttributes);
        }
    }
}