using Grpc.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Proto = Google.Protobuf.WellKnownTypes;
using MiP.Grpc.CodeGen;

namespace MiP.Grpc
{
    internal class CommandQueryMethodGenerator : MethodGenerator
    {
        private readonly IHandlerStore _handlerStore;

        public CommandQueryMethodGenerator(IMethodCodeGenerator methodCodeGenerator, IHandlerStore handlerStore)
            : base(methodCodeGenerator)
        {
            _handlerStore = handlerStore;
        }

        protected override bool CanGenerate(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var returnType = method.ReturnType;

            // methods that can be handled with IHandler<,> or ICommandHandler<> have

            // - return type of Task<T> where T is the type of the response
            // - 2 arguments where the 
            //   - first is the actual argument
            //   - second is of type ServerCallContext

            if (parameters.Length != 2) // one parameter + ServerCallContext
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

            // try to find a query or command handler
            var handlerInterface = typeof(IHandler<,>).MakeGenericType(requestType, responseType);
            var handler = _handlerStore.Find(serviceBase, handlerInterface, method.Name);

            if (handler != null)
                return GenerateMethod(Code.MethodHandlerCode, method.Name, requestType, responseType, handler);

            if (responseType != typeof(Proto.Empty))
                throw new InvalidOperationException($"Couldn't find a type that implements {handlerInterface} to handle method {method}.");

            var commandHandlerInterface = typeof(ICommandHandler<>).MakeGenericType(requestType);
            handler = _handlerStore.Find(serviceBase, commandHandlerInterface, method.Name);

            if (handler != null)
                return GenerateMethod(Code.MethodCommandHandlerCode, method.Name, requestType, responseType, handler);

            throw new InvalidOperationException($"Couldn't find a type that implements {handlerInterface} or {commandHandlerInterface} to handle method {method}.");
        }
    }
}
