using Protobuf = Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
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

        public IDispatcherMapBuilder Add<THandler>(string name, Type serviceBase)
        {
            AddInternal(typeof(THandler), name, serviceBase);

            return this;
        }

        public IDispatcherMapBuilder Add(Type handlerType, string name, Type serviceBase)
        {
            AddInternal(handlerType, name, serviceBase);

            return this;
        }

        public IDispatcherMapBuilder Add(Assembly assembly, Type serviceBase)
        {
            AddHandlersFromAssembly(assembly, serviceBase);

            return this;
        }

        public DispatcherMap FindHandlerMap(string methodName, Type parameterType, Type returnType, Type serviceBase)
        {
            // find one for the requested service
            if (_dispatcherMaps.TryGetValue(new DispatcherMapKey(methodName, parameterType, returnType, serviceBase), out var map))
                return map;

            // if not found, find one for any service
            if (_dispatcherMaps.TryGetValue(new DispatcherMapKey(methodName, parameterType, returnType, typeof(object)), out map))
                return map;

            // if return type is different to empty, there is no chance to find a match
            if (returnType != typeof(Protobuf.Empty))
                return null;

            // try to map to an ICommandHandler<T> for the requested service
            if (_dispatcherMaps.TryGetValue(new DispatcherMapKey(methodName, parameterType, typeof(void), serviceBase), out map))
                return map;

            // or one for ICommandHandler<T> for any service
            if (_dispatcherMaps.TryGetValue(new DispatcherMapKey(methodName, parameterType, typeof(void), typeof(object)), out map))
                return map;

            // nothing found that could be a match
            return null;
        }

        private void AddHandlersFromAssembly(Assembly assembly, Type serviceBase)
        {
            var handlerInfos = GetTypeInfos(assembly.GetTypes());

            foreach (var handlerInfo in handlerInfos)
            {
                Add(handlerInfo, null, serviceBase);
            }
        }

        private static IEnumerable<HandlerInfo> GetTypeInfos(IEnumerable<Type> types)
        {
            var handlerInfos = types.Select(GetIHandlers)
                .Where(hi => hi.ServiceArgs.Count > 0);

            return handlerInfos;
        }

        private static (string, Type) GetPreferredName(Type implementation)
        {
            // check class name
            string generatedName = implementation.Name;

            // remove "CommandHandler" from end
            generatedName = TrimEnd(generatedName, nameof(ICommandHandler<object>).Substring(1));

            // remove "Handler" from end
            generatedName = TrimEnd(generatedName, nameof(IHandler<object, object>).Substring(1));

            var attribute = (HandlesAttribute)implementation.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(HandlesAttribute));

            return (attribute?.MethodName ?? generatedName, attribute?.ServiceBase ?? typeof(object));
        }

        private static string TrimEnd(string from, string trim)
        {
            if (from.EndsWith(trim, StringComparison.Ordinal))
                return from.Substring(0, from.Length - trim.Length);

            return from;
        }

        private void Add(HandlerInfo handlerInfo, string name, Type serviceBase)
        {
            var (generatedName, generatedType) = GetPreferredName(handlerInfo.Implementation);

            if (string.IsNullOrEmpty(name))
                name = generatedName;

            serviceBase ??= generatedType;

            // store the AuthorizeAttributes from the implementing class
            var classAttributes = handlerInfo.Implementation.GetCustomAttributes<AuthorizeAttribute>();

            foreach (var args in handlerInfo.ServiceArgs)
            {
                Type implementation = handlerInfo.Implementation;

                var method = implementation.GetMethod(
                    nameof(IHandler<object, object>.RunAsync),
                    new[] { args.RequestType, typeof(ServerCallContext) });

                if (method == null)
                    throw new InvalidOperationException($"Could not find expected method [{Format.Method(name, args.RequestType, args.ResponseType)}]");

                // get the attributes from the method, the class could implement more than just one IHandler<,>
                var methodAuthorizeAttributes = method.GetCustomAttributes<AuthorizeAttribute>();
                var methodHandlesAttribute = method.GetCustomAttribute<HandlesAttribute>();

                var methodName = methodHandlesAttribute?.MethodName ?? name;
                var currentServiceBase = methodHandlesAttribute?.ServiceBase ?? serviceBase;

                var key = new DispatcherMapKey(methodName, args.RequestType, args.ResponseType, currentServiceBase);
                if (_dispatcherMaps.ContainsKey(key))
                {
                    Debug.WriteLine($"There is already a handler for method [{Format.Method(methodName, args.RequestType, args.ResponseType)}]. It will be removed and [{implementation.FullName}] will handle that.");
                    _dispatcherMaps.Remove(key);
                }

                var newMap = new DispatcherMap(key, implementation, classAttributes.Concat(methodAuthorizeAttributes).ToArray());
                _dispatcherMaps.Add(key, newMap);
            }
        }

        private void AddInternal(Type handlerType, string name, Type serviceBase)
        {
            var handlerInfo = GetIHandlers(handlerType);

            if (handlerInfo.ServiceArgs.Count == 0)
                throw new InvalidOperationException($"Type [{handlerType.FullName}] has no implementation of [{typeof(IHandler<,>).FullName}] or [{typeof(ICommandHandler<>).FullName}].");

            Add(handlerInfo, name, serviceBase);
        }

        private static HandlerInfo GetIHandlers(Type type)
        {
            var interfaces = type.GetInterfaces();

            var ihandlers = interfaces.Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IHandler<,>)
            )
            .Select(s =>
            {
                var typeArgs = s.GetGenericArguments();
                return new HandlerArgs(typeArgs[0], typeArgs[1]);
            });

            var icommands = interfaces.Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
            )
            .Select(s =>
            {
                var typeArgs = s.GetGenericArguments();
                return new HandlerArgs(typeArgs[0], typeof(void));
            });

            var serviceTypes = ihandlers.Concat(icommands).ToArray();

            return new HandlerInfo(type, serviceTypes);
        }
    }
}