﻿using Grpc.Core;
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

        public DispatcherMap FindHandlerMap(string methodName, Type parameterType, Type returnType)
        {
            _dispatcherMaps.TryGetValue(new DispatcherMapKey(methodName, parameterType, returnType), out var map);

            return map;
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
                .Where(hi => hi.ServiceArgs.Count > 0);

            return handlerInfos;
        }

        private void Add(HandlerInfo handlerInfo, string name)
        {
            if (string.IsNullOrEmpty(name))
                name = handlerInfo.GetPreferredName();

            // store the AuthorizeAttributes from the implementing class
            var classAttributes = handlerInfo.Implementation.GetCustomAttributes<AuthorizeAttribute>();

            foreach (var args in handlerInfo.ServiceArgs)
            {
                Type implementation = handlerInfo.Implementation;

                var key = new DispatcherMapKey(name, args.RequestType, args.ResponseType);
                if (_dispatcherMaps.ContainsKey(key))
                {
                    Debug.WriteLine($"There is already a handler for method [{Format.Method(name, args.RequestType, args.ResponseType)}]. It will be removed and [{implementation.FullName}] will handle that.");
                    _dispatcherMaps.Remove(key);
                }

                var method = implementation.GetMethod(
                    nameof(IHandler<object, object>.RunAsync), 
                    new[] { args.RequestType, typeof(ServerCallContext) });

                if (method == null)
                    throw new InvalidOperationException($"Could not find expected method [{Format.Method(name, args.RequestType, args.ResponseType)}]");

                // add the AuthorizeAttributes from the method, the class could implement more than just one IHandler<,>
                var methodAttributes = method.GetCustomAttributes<AuthorizeAttribute>();

                var newMap = new DispatcherMap(key, implementation, classAttributes.Concat(methodAttributes).ToArray());
                _dispatcherMaps.Add(key, newMap);
            }
        }

        private void AddInternal(Type handlerType, string name)
        {
            var handlerInfo = GetIHandlers(handlerType);

            if (handlerInfo.ServiceArgs.Count == 0)
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

            var serviceTypes = services.Select(s =>
            {
                var typeArgs = s.GetGenericArguments();
                return new HandlerArgs(typeArgs[0], typeArgs[1]);
            }).ToArray();

            return new HandlerInfo(type, serviceTypes);
        }
    }
}