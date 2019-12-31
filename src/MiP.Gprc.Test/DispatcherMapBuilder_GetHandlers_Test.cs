using FluentAssertions;
using Grpc.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Gprc.Test.Assembly;
using MiP.Grpc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiP.Gprc.Test
{
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
            _builder.Add(typeof(OneHandler), null);
            _builder.Add<TwoHandler>(null);
            _builder.Add(typeof(ThreeCommandHandler), null);
            _builder.Add<FourCommandHandler>(null);

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
            _builder.Add(typeof(AssemblyOneHandler).Assembly);

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
            public Task RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class FourCommandHandler : ICommandHandler<int>
        {
            public Task RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
