using FluentAssertions;
using Grpc.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace MiP.Gprc.Test
{
    [SuppressMessage("General", "RCS1079:Throwing of new NotImplementedException.", Justification = "Test classes")]
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
        public void Add_throws_when_type_added_does_not_implement_any_of_the_interfaces()
        {
            Action add = () => _builder.Add<MemoryStream>(typeof(int), "Nothing");

            add.Should().Throw<InvalidOperationException>().WithMessage($"[{typeof(MemoryStream)}] does not implement one of the handler interfaces.");
        }

        [TestMethod]
        public void Add_throws_when_handled_method_does_not_have_target_service()
        {
            var method = typeof(AllDefaultHandler).GetMethod(nameof(AllDefaultHandler.RunAsync));

            Action add = () => _builder.Add<AllDefaultHandler>(null, "Nothing");

            add.Should().Throw<InvalidOperationException>().WithMessage($"Method [{method}] on [{typeof(AllDefaultHandler)}] has no target service base.");
        }

        [TestMethod]
        public void Add_generates_default_name()
        {
            // arrange
            _builder.Add(typeof(AllDefaultHandler), typeof(IDisposable), null);

            // expected
            var expected = new HandlerInfo(typeof(AllDefaultHandler), null);

            // act trying to find by default name
            var result = _builder.Find(typeof(IDisposable), typeof(IHandler<int, string>), "AllDefault");

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_gets_name_and_service_from_attribute()
        {
            // arrange
            _builder.Add(typeof(NameAndServiceFromAttributeHandler), null, null);

            // expected
            var expected = new HandlerInfo(typeof(NameAndServiceFromAttributeHandler), null);

            // act trying to find by default name
            var result = _builder.Find(typeof(string), typeof(IHandler<string, int>), "Fourth");

            // assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Add_finds_multiple_implementations_using_attribute_on_method()
        {
            // arrange
            _builder.Add(typeof(MultipleWithAttributesHandler), null, null);

            // expected
            var expected1 = new HandlerInfo(typeof(MultipleWithAttributesHandler), null);
            var expected2 = new HandlerInfo(typeof(MultipleWithAttributesHandler), null);

            // act trying to find by default name
            var result1 = _builder.Find(typeof(string), typeof(IHandler<string, int>), "OneMethod");
            var result2 = _builder.Find(typeof(int), typeof(IHandler<int, string>), "TwoMethod");

            // assert
            result1.Should().BeEquivalentTo(expected1);
            result2.Should().BeEquivalentTo(expected2);
        }

        [TestMethod]
        public void Find_throws_when_serviceBase_is_null()
        {
            Action find = () => _builder.Find(null, typeof(int), "method");

            find.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("serviceBase");
        }

        [TestMethod]
        public void Find_throws_when_handlerInterface_is_null()
        {
            Action find = () => _builder.Find(typeof(int), null, "method");

            find.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("handlerInterface");
        }

        [TestMethod]
        public void Find_throws_when_methodName_is_null()
        {
            Action find = () => _builder.Find(typeof(int), typeof(int), null);

            find.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("methodName");
        }

        [TestMethod]
        public void Find_throws_when_handlerInterface_is_not_a_HandlerInterface()
        {
            Action find = () => _builder.Find(typeof(int), typeof(int), "method");

            find.Should().Throw<ArgumentException>()
                .WithMessage($"Argument handlerInterface must be one of the handler interfaces.*")
                .And.ParamName.Should().Be("handlerInterface");
        }

        private class AllDefaultHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
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

        private class MultipleWithAttributesHandler : IHandler<string, int>, IHandler<int, string>
        {
            [Handles("OneMethod", typeof(string))]
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
