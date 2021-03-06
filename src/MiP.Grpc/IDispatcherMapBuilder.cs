﻿using System;
using System.Reflection;

namespace MiP.Grpc
{
    /// <summary>
    /// Used to register handlers for the dispatcher.
    /// </summary>
    public interface IDispatcherMapBuilder
    {
        /// <summary>
        /// Adds the <typeparamref name="THandler"/> to the map.
        /// </summary>
        /// <typeparam name="THandler">The type of the handler that's added to the map.</typeparam>
        /// <param name="serviceBase">Type of the service whose method is handled.</param>
        /// <param name="methodName">The name of the service method the <typeparamref name="THandler"/> handles.</param>
        /// <returns><see cref="IDispatcherMapBuilder"/>.</returns>
        IDispatcherMapBuilder Add<THandler>(Type serviceBase = null, string methodName = null);

        /// <summary>
        /// Adds the <paramref name="handlerType"/> to the map.
        /// </summary>
        /// <param name="handlerType">The type of the handler that's added to the map.</param>
        /// <param name="serviceBase">Type of the service whose method is handled.</param>
        /// <param name="methodName">The name of the service method the <paramref name="handlerType"/> handles.</param>
        /// <returns><see cref="IDispatcherMapBuilder"/>.</returns>
        IDispatcherMapBuilder Add(Type handlerType, Type serviceBase = null, string methodName = null);

        /// <summary>
        /// Adds all types from the assembly that implement <see cref="IHandler{TRequest, TResponse}"/> to the map.
        /// </summary>
        /// <param name="assembly">All types of this assembly are added to the map.</param>
        /// <param name="serviceBase">Type of the service whose method is handled by classes in <paramref name="assembly"/>.</param>
        /// <returns><see cref="IDispatcherMapBuilder"/>.</returns>
        IDispatcherMapBuilder Add(Assembly assembly, Type serviceBase = null);
    }
}