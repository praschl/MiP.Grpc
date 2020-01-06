using Grpc.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MiP.Grpc.CodeGen;

namespace MiP.Grpc
{
    internal class ClientStreamMethodGenerator : MethodGenerator
    {
        private readonly IHandlerStore _handlerStore;

        public ClientStreamMethodGenerator(IMethodCodeGenerator methodCodeGenerator, IHandlerStore handlerStore)
            : base(methodCodeGenerator)
        {
            _handlerStore = handlerStore;
        }

        protected override bool CanGenerate(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var returnType = method.ReturnType;

            if (parameters.Length != 2) // one parameter + ServerCallContext
                return false;

            var parameter1Type = parameters[0].ParameterType;
            if (!parameter1Type.IsGenericType) // parameter1Type is Task<T> where T is the actual stream type
                return false;

            if (parameter1Type.GetGenericTypeDefinition() != typeof(IAsyncStreamReader<>))
                return false;

            if (parameters[1].ParameterType != typeof(ServerCallContext))
                return false;

            if (!returnType.IsGenericType) // returnTaskType is Task<T> where T is the actual return type
                return false;

            if (returnType.GetGenericTypeDefinition() != typeof(Task<>))
                return false;

            return true;
        }

        protected override GeneratedMethod GenerateCode(Type serviceBase, MethodInfo method)
        {
            var parameters = method.GetParameters();

            var requestType = parameters[0].ParameterType;
            var responseType = method.ReturnType.GetGenericArguments().Single(); // get the actual return type
            var streamParameterType = requestType.GetGenericArguments().Single();

            // try to find a query with IAsyncStreamReader
            var handlerInterface = typeof(IClientStreamHandler<,>).MakeGenericType(streamParameterType, responseType);
            var handler = _handlerStore.Find(serviceBase, handlerInterface, method.Name);

            if (handler == null)
                throw new InvalidOperationException($"Couldn't find a type that implements {handlerInterface} to handle method {method}.");

            return GenerateMethod(Code.MethodClientStreamHandlerCode, method.Name, streamParameterType, responseType, handler);
        }
    }
}
