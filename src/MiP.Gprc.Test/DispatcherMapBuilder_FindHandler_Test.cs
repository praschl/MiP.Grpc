using FluentAssertions;
using Protobuf = Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class DispatcherMapBuilder_Add_Test
    {
        private DispatcherMapBuilder _builder;

        [TestInitialize]
        public void Initialize()
        {
            _builder = new DispatcherMapBuilder();
        }

        [TestMethod]
        public void Add_generates_default_name()
        {
            // arrange
            _builder.Add(typeof(OneHandler), null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("One", typeof(int), typeof(string)), typeof(OneHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("One", typeof(int), typeof(string));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_generates_name_from_attribute()
        {
            // arrange
            _builder.Add(typeof(TwoHandler), null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("Second", typeof(string), typeof(int)), typeof(TwoHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("Second", typeof(string), typeof(int));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_generates_name_from_parameter()
        {
            // arrange
            _builder.Add(typeof(TwoHandler), "OneMore");

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("OneMore", typeof(string), typeof(int)), typeof(TwoHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("OneMore", typeof(string), typeof(int));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_generates_default_name_for_CommandHandler()
        {
            // arrange
            _builder.Add(typeof(ThreeCommandHandler), null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("Three", typeof(int), typeof(void)), typeof(ThreeCommandHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("Three", typeof(int), typeof(Protobuf.Empty));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_finds_multiple_implementations()
        {
            // arrange
            _builder.Add(typeof(MultipleHandler), null);

            // expected
            var expected1 = new DispatcherMap(new DispatcherMapKey("Multiple", typeof(int), typeof(string)), typeof(MultipleHandler), new AuthorizeAttribute[0]);
            var expected2 = new DispatcherMap(new DispatcherMapKey("Multiple", typeof(string), typeof(int)), typeof(MultipleHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result1 = _builder.FindHandlerMap("Multiple", typeof(int), typeof(string));
            var result2 = _builder.FindHandlerMap("Multiple", typeof(string), typeof(int));

            // assert
            result1.Should().BeEquivalentTo(expected1);
            result2.Should().BeEquivalentTo(expected2);
        }

        [TestMethod]
        public void Add_finds_multiple_implementations_using_attribute_on_method()
        {
            // arrange
            _builder.Add(typeof(MultipleWithAttributesHandler), null);

            // expected
            var expected1 = new DispatcherMap(new DispatcherMapKey("OneMethod", typeof(string), typeof(int)), typeof(MultipleWithAttributesHandler), new AuthorizeAttribute[0]);
            var expected2 = new DispatcherMap(new DispatcherMapKey("TwoMethod", typeof(int), typeof(string)), typeof(MultipleWithAttributesHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result1 = _builder.FindHandlerMap("OneMethod", typeof(string), typeof(int));
            var result2 = _builder.FindHandlerMap("TwoMethod", typeof(int), typeof(string));

            // assert
            result1.Should().BeEquivalentTo(expected1);
            result2.Should().BeEquivalentTo(expected2);
        }

        private class OneHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Handles("Second")]
        private class TwoHandler : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class ThreeCommandHandler : ICommandHandler<int>
        {
            public Task RunAsync(int command, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class MultipleHandler : IHandler<string, int>, IHandler<int, string>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }

            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class MultipleWithAttributesHandler : IHandler<string, int>, IHandler<int, string>
        {
            [Handles("OneMethod")]
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }

            [Handles("TwoMethod")]
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
