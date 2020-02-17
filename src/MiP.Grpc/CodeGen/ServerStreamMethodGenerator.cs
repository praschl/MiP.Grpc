using Grpc.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MiP.Grpc.CodeGen;

namespace MiP.Grpc
{
    internal class ServerStreamMethodGenerator : MethodGenerator
    {
        private readonly IHandlerStore _handlerStore;

        public ServerStreamMethodGenerator(IMethodCodeGenerator methodCodeGenerator, IHandlerStore handlerStore)
            : base(methodCodeGenerator)
        {
            _handlerStore = handlerStore;
        }

        protected override bool CanGenerate(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var returnType = method.ReturnType;

            if (parameters.Length != 3) // request, IServerStreamWriter<T>, ServerCallContext
                return false;

            if (parameters[2].ParameterType != typeof(ServerCallContext))
                return false;

            if (returnType != typeof(Task)) // server streaming has no return type
                return false;

            var parameter1Type = parameters[1].ParameterType;
            if (!parameter1Type.IsGenericType) // parameter1Type is Task<T> where T is the actual stream type
                return false;

            if (parameter1Type.GetGenericTypeDefinition() != typeof(IServerStreamWriter<>))
                return false;

            return true;
        }

        protected override GeneratedMethod GenerateCode(Type serviceBase, MethodInfo method)
        {
            var parameters = method.GetParameters();

            var parameterType = parameters[0].ParameterType;
            var streamType = parameters[1].ParameterType.GetGenericArguments().Single();

            var handlerInterface = typeof(IServerStreamHandler<,>).MakeGenericType(parameterType, streamType);
            var handler = _handlerStore.Find(serviceBase, handlerInterface, method.Name);

            if (handler == null)
                throw new InvalidOperationException($"Couldn't find a type that implements {handlerInterface} to handle method {method}.");

            return GenerateMethod(Code.MethodServerStreamHandlerCode, method.Name, parameterType, streamType, handler);
        }
    }

    internal class BidiStreamMethodGenerator : MethodGenerator
    {
        private readonly IHandlerStore _handlerStore;

        public BidiStreamMethodGenerator(IMethodCodeGenerator methodCodeGenerator, IHandlerStore handlerStore)
            : base(methodCodeGenerator)
        {
            _handlerStore = handlerStore;
        }

        protected override bool CanGenerate(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var returnType = method.ReturnType;

            if (parameters.Length != 3) // IAsyncStreamReader<T>, IServerStreamWriter<T>, ServerCallContext
                return false;

            if (parameters[2].ParameterType != typeof(ServerCallContext))
                return false;

            if (returnType != typeof(Task)) // server streaming has no return type
                return false;

            var parameter0Type = parameters[0].ParameterType;
            if (!parameter0Type.IsGenericType) // parameter1Type is Task<T> where T is the actual stream type
                return false;

            if (parameter0Type.GetGenericTypeDefinition() != typeof(IAsyncStreamReader<>))
                return false;

            var parameter1Type = parameters[1].ParameterType;
            if (!parameter1Type.IsGenericType) // parameter1Type is Task<T> where T is the actual stream type
                return false;

            if (parameter1Type.GetGenericTypeDefinition() != typeof(IServerStreamWriter<>))
                return false;

            return true;
        }

        protected override GeneratedMethod GenerateCode(Type serviceBase, MethodInfo method)
        {
            var parameters = method.GetParameters();

            var requestType = parameters[0].ParameterType.GetGenericArguments().Single();
            var responseType = parameters[1].ParameterType.GetGenericArguments().Single();

            var handlerInterface = typeof(IBidiStreamHandler<,>).MakeGenericType(requestType, responseType);
            var handler = _handlerStore.Find(serviceBase, handlerInterface, method.Name);

            if (handler == null)
                throw new InvalidOperationException($"Couldn't find a type that implements {handlerInterface} to handle method {method}.");

            return GenerateMethod(Code.MethodBidiStreamHandlerCode, method.Name, requestType, responseType, handler);

        }
    }
}
