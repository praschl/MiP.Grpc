using Microsoft.AspNetCore.Authorization;
using MiP.Grpc.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiP.Grpc
{
    internal static class TypeExtensions
    {
        private const string Base = "Base";
        private const string Dispatcher = "Dispatcher";

        private static readonly string HandlerName = nameof(IHandler<object, object>)[1..];

        public static readonly IReadOnlyList<Type> HandlerInterfaces = new[]
        {
            typeof(IHandler<,>),
            typeof(ICommandHandler<>),
            typeof(IServerStreamHandler<,>),
            typeof(IClientStreamHandler<,>),
            typeof(IBidiStreamHandler<,>),
        };

        public static bool HasHandlerInterface(this Type candidate)
        {
            // just check if at least one of the interfaces is implemented
            return candidate.GetHandlerInterfaces().Any();
        }

        public static IEnumerable<Type> GetHandlerInterfaces(this Type candidate)
        {
            var ifaces = candidate.GetInterfaces();
            return ifaces.Where(IsHandlerInterface);
        }

        public static bool IsHandlerInterface(this Type candidate)
        {
            if (candidate.ContainsGenericParameters) // when open generic
                return false;

            return candidate.IsGenericType
                && HandlerInterfaces.Contains(candidate.GetGenericTypeDefinition());
        }

        public static HandlesAttribute GetHandlesAttribute(this Type handler, Type iface)
        {
            return handler.GetInterfaceMap(iface).TargetMethods[0].GetCustomAttribute<HandlesAttribute>();
        }

        public static HandlesAttribute GetHandlesAttribute(this Type handler)
        {
            return handler.GetCustomAttribute<HandlesAttribute>();
        }

        public static IReadOnlyCollection<AuthorizeAttribute> GetAuthorizeAttributes(this Type handler, Type handlerInterface)
        {
            var classAttributes = handler.GetCustomAttributes<AuthorizeAttribute>();
            var methodAttributes = handler.GetInterfaceMap(handlerInterface).TargetMethods[0].GetCustomAttributes<AuthorizeAttribute>();

            return classAttributes.Concat(methodAttributes).ToArray();
        }

        public static string GetHandlerNameFromClass(this Type handler, Type iface)
        {
            var name = handler.Name;

            // remove "I" prefix as well as `1 or `2 from end
            var ifaceName = iface.Name[1..^2];
            var trimmed = TrimEnd(name, ifaceName);

            if (trimmed == name) // if nothing was removed
            {
                trimmed = TrimEnd(name, HandlerName);
            }

            return trimmed;
        }

        public static string GetMethodName(this Type handler, Type iface)
        {
            string methodName = handler.GetHandlesAttribute(iface)?.MethodName;
            if (!string.IsNullOrWhiteSpace(methodName))
                return methodName;

            methodName = handler.GetHandlesAttribute()?.MethodName;
            if (!string.IsNullOrWhiteSpace(methodName))
                return methodName;

            return GetHandlerNameFromClass(handler, iface);
        }

        public static Type GetServiceBase(this Type handlerType, Type iface)
        {
            return
                handlerType.GetHandlesAttribute(iface)?.ServiceBase
                ??
                handlerType.GetHandlesAttribute()?.ServiceBase;
        }

        public static string GetFullClassName(this Type type)
        {
            return Code.GlobalPrefix + type.FullName.Replace('+', '.'); // + is used by framework for nested classes
        }

        public static string GetGeneratedDispatcherName(this Type serviceBaseType)
        {
            var className = serviceBaseType.Name;

            if (className.EndsWith(Base, StringComparison.Ordinal)) // which it does from the protobuf code generation
                className = className.Substring(0, className.Length - Base.Length);

            className += Dispatcher;

            return className;
        }

        private static string TrimEnd(string from, string trim)
        {
            if (from.EndsWith(trim, StringComparison.Ordinal))
                return from.Substring(0, from.Length - trim.Length);

            return from;
        }
    }
}