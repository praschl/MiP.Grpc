using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    public class DispatcherCompiler
    {
        private class Tag
        {
            public const string Class = "{Class}";
            public const string Members = "{Members}";

            public const string Constructor = "{Constructor}";
            public const string Method = "{Method}";
            public const string Request = "{Request}";
            public const string Reply = "{Reply}";
        }

        public const string ClassCode = @"
public class " + Tag.Class + @" 
{
    " + Tag.Members + @"
}
";

        public const string ConstructorCode = @"
    private readonly IServiceProvider _serviceProvider;
    public " + Tag.Constructor + @"(IServiceProvider serviceProvider) 
    {
        _serviceProvider = serviceProvider;
    }
";

        public const string MethodCode = @"
    public async Task<" + Tag.Reply + "> " + Tag.Method + "(" + Tag.Reply + @" reply)
    {
        await Task.CompletedTask;
    }
";

        private const string Base = "Base";

        public Type CompileDispatcher(Type serviceBase)
        {
            var definitions = GetMethodsToImplement(serviceBase);
            string implName = serviceBase.Name;
            if (implName.EndsWith(Base))
                implName = implName.Substring(0, implName.Length - Base.Length);
            implName += "Dispatcher";

            var source = GenerateSource(definitions, implName);

            Console.WriteLine("---------------------------------------");
            Console.WriteLine(source);
            Console.WriteLine("---------------------------------------");

            return null;
        }

        private string GenerateSource(IEnumerable<QueryDefinition> definitions, string typeName)
        {
            string members =
                ConstructorCode.Replace("{ConstructorName}", typeName)
                +
                string.Concat(definitions.Select(GenerateMethod));

            string classSource = ClassCode
                .Replace(Tag.Class, typeName)
                .Replace(Tag.Members, members);

            return classSource;
        }

        private string GenerateMethod(QueryDefinition definition)
        {
            return MethodCode
                .Replace(Tag.Method, definition.MethodName)
                .Replace(Tag.Request, definition.RequestType.Name)
                .Replace(Tag.Reply, definition.ReplyType.Name);
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
