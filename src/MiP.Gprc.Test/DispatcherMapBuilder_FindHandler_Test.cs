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
        public void Add_generates_default_name_and_default_service()
        {
            // arrange
            _builder.Add(typeof(AllDefaultHandler), null, null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("AllDefault", typeof(int), typeof(string), typeof(object)), typeof(AllDefaultHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("AllDefault", typeof(int), typeof(string), typeof(object));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_generates_default_name_and_gets_service_from_attribute()
        {
            // arrange
            _builder.Add(typeof(DefaultName_ServiceFromAttributeHandler), null, null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("DefaultName_ServiceFromAttribute", typeof(int), typeof(string), typeof(string)), typeof(DefaultName_ServiceFromAttributeHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("DefaultName_ServiceFromAttribute", typeof(int), typeof(string), typeof(string));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_generates_default_name_and_gets_service_from_parameter()
        {
            // arrange
            _builder.Add(typeof(AllDefaultHandler), null, typeof(string));

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("AllDefault", typeof(int), typeof(string), typeof(string)), typeof(AllDefaultHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("AllDefault", typeof(int), typeof(string), typeof(string));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_gets_name_from_attribute()
        {
            // arrange
            _builder.Add(typeof(NameFromAttributeHandler), null, null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("Second", typeof(string), typeof(int), typeof(object)), typeof(NameFromAttributeHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("Second", typeof(string), typeof(int), typeof(object));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_gets_name_and_service_from_attribute()
        {
            // arrange
            _builder.Add(typeof(NameAndServiceFromAttributeHandler), null, null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("Fourth", typeof(string), typeof(int), typeof(string)), typeof(NameAndServiceFromAttributeHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("Fourth", typeof(string), typeof(int), typeof(string));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_generates_name_from_parameter()
        {
            // arrange
            _builder.Add(typeof(NameFromAttributeHandler), "OneMore", null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("OneMore", typeof(string), typeof(int), typeof(object)), typeof(NameFromAttributeHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("OneMore", typeof(string), typeof(int), typeof(object));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_gets_name_and_service_from_parameter()
        {
            // arrange
            _builder.Add(typeof(NameAndServiceFromAttributeHandler), "FromParam", typeof(long));

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("FromParam", typeof(string), typeof(int), typeof(long)), typeof(NameAndServiceFromAttributeHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("FromParam", typeof(string), typeof(int), typeof(long));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_generates_default_name_for_CommandHandler()
        {
            // arrange
            _builder.Add(typeof(DefaultNameCommandHandler), null, null);

            // expected
            var expected = new DispatcherMap(new DispatcherMapKey("DefaultName", typeof(int), typeof(void), typeof(object)), typeof(DefaultNameCommandHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result = _builder.FindHandlerMap("DefaultName", typeof(int), typeof(Protobuf.Empty), typeof(object));

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_finds_multiple_implementations()
        {
            // arrange
            _builder.Add(typeof(MultipleHandler), null, null);

            // expected
            var expected1 = new DispatcherMap(new DispatcherMapKey("Multiple", typeof(int), typeof(string), typeof(object)), typeof(MultipleHandler), new AuthorizeAttribute[0]);
            var expected2 = new DispatcherMap(new DispatcherMapKey("Multiple", typeof(string), typeof(int), typeof(object)), typeof(MultipleHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result1 = _builder.FindHandlerMap("Multiple", typeof(int), typeof(string), typeof(object));
            var result2 = _builder.FindHandlerMap("Multiple", typeof(string), typeof(int), typeof(object));

            // assert
            result1.Should().BeEquivalentTo(expected1);
            result2.Should().BeEquivalentTo(expected2);
        }

        [TestMethod]
        public void Add_finds_multiple_implementations_using_attribute_on_method()
        {
            // arrange
            _builder.Add(typeof(MultipleWithAttributesHandler), null, null);

            // expected
            var expected1 = new DispatcherMap(new DispatcherMapKey("OneMethod", typeof(string), typeof(int), typeof(object)), typeof(MultipleWithAttributesHandler), new AuthorizeAttribute[0]);
            var expected2 = new DispatcherMap(new DispatcherMapKey("TwoMethod", typeof(int), typeof(string), typeof(int)), typeof(MultipleWithAttributesHandler), new AuthorizeAttribute[0]);

            // act trying to find by default name
            var result1 = _builder.FindHandlerMap("OneMethod", typeof(string), typeof(int), typeof(object));
            var result2 = _builder.FindHandlerMap("TwoMethod", typeof(int), typeof(string), typeof(int));

            // assert
            result1.Should().BeEquivalentTo(expected1);
            result2.Should().BeEquivalentTo(expected2);
        }

        private class AllDefaultHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Handles("Second")]
        private class NameFromAttributeHandler : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Handles("Fourth", ServiceBase = typeof(string))]
        private class NameAndServiceFromAttributeHandler : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Handles(ServiceBase = typeof(string))]
        private class DefaultName_ServiceFromAttributeHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class DefaultNameCommandHandler : ICommandHandler<int>
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

            [Handles("TwoMethod", typeof(int))]
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
