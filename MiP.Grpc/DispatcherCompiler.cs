using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    public class DispatcherCompiler
    {
        public Type CompileDispatcher(Type serviceBase)
        {
            Console.WriteLine("------------");
            var definitions = GetMethodsToImplement(serviceBase);

            foreach (var def in definitions)
            {
                Console.WriteLine($"{def.ReplyType.Name} {def.MethodName}({def.RequestType.Name})");
            }

            Console.WriteLine("------------");

            return null;
        }

        private static IEnumerable<QueryDefinition> GetMethodsToImplement(Type serviceBase)
        {
            var methods = serviceBase.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (method.ReturnType == typeof(void))
                    continue;

                var parameters = method.GetParameters();

                if (parameters.Length != 2)
                    continue;

                if (parameters[1].ParameterType != typeof(ServerCallContext))
                    continue;

                var returnType = method.ReturnType;
                if (!returnType.IsGenericType)
                    continue;

                if (returnType.GetGenericTypeDefinition() != typeof(Task<>))
                    continue;

                var returnTypeArgument = returnType.GetGenericArguments().Single();

                var result = new QueryDefinition(method.Name, parameters[0].ParameterType, returnTypeArgument);
                yield return result;
            }
        }
    }

    public class QueryDefinition
    {
        public QueryDefinition(string methodName, Type requestType, Type replyType)
        {
            MethodName = methodName;
            RequestType = requestType;
            ReplyType = replyType;
        }

        public string MethodName { get; }
        public Type RequestType { get; }
        public Type ReplyType { get; }
    }
}
