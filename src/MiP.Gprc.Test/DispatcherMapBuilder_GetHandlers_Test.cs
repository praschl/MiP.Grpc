using FluentAssertions;
using Grpc.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Gprc.Test.Assembly;
using MiP.Grpc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MiP.Gprc.Test
{
    [SuppressMessage("General", "RCS1079:Throwing of new NotImplementedException.", Justification = "Test classes")]
    [TestClass]
    public class DispatcherMapBuilder_GetHandlers_Test
    {
        private DispatcherMapBuilder _builder;

        [TestInitialize]
        public void Initialize()
        {
            _builder = new DispatcherMapBuilder();
        }

        [TestMethod]
        public void GetHandlers_returns_the_added_concrete_types()
        {
            // arrange
            _builder.Add(typeof(OneHandler), typeof(IDisposable), null);
            _builder.Add<TwoHandler>(typeof(IDisposable), null);
            _builder.Add(typeof(ThreeCommandHandler), typeof(IDisposable), null);
            _builder.Add(typeof(ThreeCommandHandler), typeof(MemoryStream), null); // even when registered for another service there should not be a duplicate returned
            _builder.Add<FourCommandHandler>(typeof(IDisposable), null);

            // expect
            Type[] expectedTypes = { typeof(OneHandler), typeof(TwoHandler), typeof(ThreeCommandHandler), typeof(FourCommandHandler) };

            // act
            var handlers = _builder.GetHandlers().ToArray();

            //assert
            handlers.Should().BeEquivalentTo(expectedTypes);
        }

        [TestMethod]
        public void GetHandlers_returns_concrete_types_added_from_assembly()
        {
            // arrange
            _builder.Add(typeof(AssemblyOneHandler).Assembly, typeof(IDisposable));

            // expect
            Type[] expectedTypes = { typeof(AssemblyOneHandler), typeof(AssemblyTwoHandler), typeof(AssemblyThreeCommandHandler), typeof(AssemblyFourCommandHandler) };

            // act
            var handlers = _builder.GetHandlers().ToArray();

            //assert
            handlers.Should().BeEquivalentTo(expectedTypes);
        }

        private class OneHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class TwoHandler : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class ThreeCommandHandler : ICommandHandler<string>
        {
            public Task RunAsync(string command, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class FourCommandHandler : ICommandHandler<int>
        {
            public Task RunAsync(int command, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
