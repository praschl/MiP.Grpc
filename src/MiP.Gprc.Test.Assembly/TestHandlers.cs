using Grpc.Core;
using MiP.Grpc;
using System;
using System.Threading.Tasks;

namespace MiP.Gprc.Test.Assembly
{
    public class AssemblyOneHandler : IHandler<string, int>
    {
        public Task<int> RunAsync(string request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class AssemblyTwoHandler : IHandler<int, string>
    {
        public Task<string> RunAsync(int request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class AssemblyThreeCommandHandler : ICommandHandler<string>
    {
        public Task RunAsync(string command, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class AssemblyFourCommandHandler : ICommandHandler<int>
    {
        public Task RunAsync(int command, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }
}
