using FakeItEasy;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Protobuf = Google.Protobuf.WellKnownTypes;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class DispatcherCompilerTest
    {
        private ServerCallContext _callContext;
        private IHandlerStore _handler;
        private MapGrpcServiceConfiguration _config;
        private DispatcherCompiler _compiler;

        private Guid _guid = Guid.NewGuid();

        [TestInitialize]
        public void Initialize()
        {
            _callContext = TestServerCallContext.Create(_guid.ToString(),
                null, DateTime.Now, null, CancellationToken.None, null, null, null, null, null, null);

            _handler = A.Fake<IHandlerStore>();
            _config = new MapGrpcServiceConfiguration();

            _compiler = new DispatcherCompiler(_handler, _config);
        }

        [TestMethod]
        public void Compiles_class_with_constructor()
        {
            // act
            var result = _compiler.CompileDispatcher(typeof(EmptyBase));

            // assert
            result.Should().NotBeNull();
            result.Should().HaveConstructor(new[] { typeof(IDispatcher) });
        }

        [TestMethod]
        public void Adds_AuthorizeAttributes_to_class()
        {
            // arrange
            _config.AddGlobalAuthorizeAttribute("my.policy.1", "my.roles.1", "my.scheme.1");
            _config.AddGlobalAuthorizeAttribute("my.policy.2", "my.roles.2", "my.scheme.2");

            // expected
            var authorize1 = new AuthorizeAttribute("my.policy.1") { Roles = "my.roles.1", AuthenticationSchemes = "my.scheme.1" };
            var authorize2 = new AuthorizeAttribute("my.policy.2") { Roles = "my.roles.2", AuthenticationSchemes = "my.scheme.2" };

            // act
            var result = _compiler.CompileDispatcher(typeof(EmptyBase));

            // assert
            var attributes = result.GetCustomAttributes<AuthorizeAttribute>();
            attributes.Should().BeEquivalentTo(new[] { authorize1, authorize2 });

            result.Should().NotBe<EmptyBase>();
            result.Should().BeAssignableTo<EmptyBase>();
        }

        [TestMethod]
        public void Adds_methods_for_IHandler()
        {
            // arrange
            A.CallTo(() => _handler.FindHandlerMap(nameof(TwoHandlers.One), typeof(string), typeof(int)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(TwoHandlers.One), typeof(string), typeof(int)), typeof(StringIntHandler), new AuthorizeAttribute[0]));
            A.CallTo(() => _handler.FindHandlerMap(nameof(TwoHandlers.Two), typeof(int), typeof(string)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(TwoHandlers.Two), typeof(int), typeof(string)), typeof(IntStringHandler), new AuthorizeAttribute[0]));

            // act
            var result = _compiler.CompileDispatcher(typeof(TwoHandlers));

            // assert
            result.Should().HaveMethod(nameof(TwoHandlers.One), new[] { typeof(string), typeof(ServerCallContext) }).Which.ReturnType.Should().Be(typeof(Task<int>));
            result.Should().HaveMethod(nameof(TwoHandlers.Two), new[] { typeof(int), typeof(ServerCallContext) }).Which.ReturnType.Should().Be(typeof(Task<string>));
        }

        [TestMethod]
        public async Task Execution_of_methods_for_IHandler()
        {
            // arrange
            A.CallTo(() => _handler.FindHandlerMap(nameof(TwoHandlers.One), typeof(string), typeof(int)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(TwoHandlers.One), typeof(string), typeof(int)), typeof(StringIntHandler), new AuthorizeAttribute[0]));
            A.CallTo(() => _handler.FindHandlerMap(nameof(TwoHandlers.Two), typeof(int), typeof(string)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(TwoHandlers.Two), typeof(int), typeof(string)), typeof(IntStringHandler), new AuthorizeAttribute[0]));

            var dispatcher = A.Fake<IDispatcher>();
            A.CallTo(() => dispatcher.Dispatch<string, int, StringIntHandler>("request one", _callContext, "One"))
                .Returns(Task.FromResult(14));
            A.CallTo(() => dispatcher.Dispatch<int, string, IntStringHandler>(2, _callContext, "Two"))
                .Returns(Task.FromResult("response two"));

            var compiledType = _compiler.CompileDispatcher(typeof(TwoHandlers));
            var result = Activator.CreateInstance(compiledType, dispatcher) as TwoHandlers;

            // act
            var executionResult1 = await result.One("request one", _callContext);
            var executionResult2 = await result.Two(2, _callContext);

            // assert
            executionResult1.Should().Be(14);
            executionResult2.Should().Be("response two");
        }

        [TestMethod]
        public void Adds_methods_for_IHandler_with_Empty()
        {
            // arrange
            A.CallTo(() => _handler.FindHandlerMap(nameof(EmptyHandlers.One), typeof(Protobuf.Empty), typeof(Protobuf.Empty)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(EmptyHandlers.One), typeof(Protobuf.Empty), typeof(Protobuf.Empty)), typeof(EmptyOneHandler), new AuthorizeAttribute[0]));
            A.CallTo(() => _handler.FindHandlerMap(nameof(EmptyHandlers.Two), typeof(Protobuf.Empty), typeof(Protobuf.Empty)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(EmptyHandlers.Two), typeof(Protobuf.Empty), typeof(Protobuf.Empty)), typeof(EmptyTwoHandler), new AuthorizeAttribute[0]));

            // act
            var result = _compiler.CompileDispatcher(typeof(EmptyHandlers));

            // assert
            result.Should().HaveMethod(nameof(EmptyHandlers.One), new[] { typeof(Protobuf.Empty), typeof(ServerCallContext) }).Which.ReturnType.Should().Be(typeof(Task<Protobuf.Empty>));
            result.Should().HaveMethod(nameof(EmptyHandlers.Two), new[] { typeof(Protobuf.Empty), typeof(ServerCallContext) }).Which.ReturnType.Should().Be(typeof(Task<Protobuf.Empty>));
        }

        [TestMethod]
        public async Task Execution_of_methods_for_IHandler_with_Empty()
        {
            // arrange
            A.CallTo(() => _handler.FindHandlerMap(nameof(EmptyHandlers.One), typeof(Protobuf.Empty), typeof(Protobuf.Empty)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(EmptyHandlers.One), typeof(Protobuf.Empty), typeof(Protobuf.Empty)), typeof(EmptyOneHandler), new AuthorizeAttribute[0]));
            A.CallTo(() => _handler.FindHandlerMap(nameof(EmptyHandlers.Two), typeof(Protobuf.Empty), typeof(Protobuf.Empty)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(EmptyHandlers.Two), typeof(Protobuf.Empty), typeof(Protobuf.Empty)), typeof(EmptyTwoHandler), new AuthorizeAttribute[0]));

            var dispatcher = A.Fake<IDispatcher>();
            A.CallTo(() => dispatcher.Dispatch<Protobuf.Empty, Protobuf.Empty, EmptyOneHandler>(new Protobuf.Empty(), _callContext, "One"))
                .Returns(Task.FromResult(new Protobuf.Empty()));
            A.CallTo(() => dispatcher.Dispatch<Protobuf.Empty, Protobuf.Empty, EmptyTwoHandler>(new Protobuf.Empty(), _callContext, "Two"))
                .Returns(Task.FromResult(new Protobuf.Empty()));

            var compiledType = _compiler.CompileDispatcher(typeof(EmptyHandlers));
            var result = Activator.CreateInstance(compiledType, dispatcher) as EmptyHandlers;

            // act
            var executionResult1 = await result.One(new Protobuf.Empty(), _callContext);
            var executionResult2 = await result.Two(new Protobuf.Empty(), _callContext);

            // assert
            executionResult1.Should().Be(new Protobuf.Empty());
            executionResult2.Should().Be(new Protobuf.Empty());

            A.CallTo(() => dispatcher.Dispatch<Protobuf.Empty, Protobuf.Empty, EmptyOneHandler>(new Protobuf.Empty(), _callContext, "One"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => dispatcher.Dispatch<Protobuf.Empty, Protobuf.Empty, EmptyTwoHandler>(new Protobuf.Empty(), _callContext, "Two"))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void Adds_AuthorizeAttribute_to_IHandler_methods()
        {
            // expected
            var methodOneAttributes = new[]
            {
                new AuthorizeAttribute("my.policy.1") { Roles = "my.role.1", AuthenticationSchemes = "my.scheme.1" },
                new AuthorizeAttribute("my.policy.2") { Roles = "my.role.2", AuthenticationSchemes = "my.scheme.2" },
            };

            var methodTwoAttributes = new[]
            {
                new AuthorizeAttribute("my.policy.3") { Roles = "my.role.3", AuthenticationSchemes = "my.scheme.3" },
                new AuthorizeAttribute("my.policy.4") { Roles = "my.role.4", AuthenticationSchemes = "my.scheme.4" },
            };

            // arrange
            A.CallTo(() => _handler.FindHandlerMap(nameof(TwoHandlers.One), typeof(string), typeof(int)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(TwoHandlers.One), typeof(string), typeof(int)), typeof(StringIntHandler), methodOneAttributes));
            A.CallTo(() => _handler.FindHandlerMap(nameof(TwoHandlers.Two), typeof(int), typeof(string)))
                .Returns(new DispatcherMap(new DispatcherMapKey(nameof(TwoHandlers.Two), typeof(int), typeof(string)), typeof(IntStringHandler), methodTwoAttributes));

            // act
            var result = _compiler.CompileDispatcher(typeof(TwoHandlers));

            // assert
            result.GetMethod(nameof(TwoHandlers.One)).GetCustomAttributes<AuthorizeAttribute>().Should().BeEquivalentTo(methodOneAttributes);
            result.GetMethod(nameof(TwoHandlers.Two)).GetCustomAttributes<AuthorizeAttribute>().Should().BeEquivalentTo(methodTwoAttributes);
        }

        // TODO: same tests for ICommandHandler

        public class EmptyBase { }

        public class TwoHandlers
        {
            public virtual Task<int> One(string request, ServerCallContext context)
            {
                return Task.FromResult(0);
            }

            public virtual Task<string> Two(int request, ServerCallContext context)
            {
                return Task.FromResult<string>(null);
            }
        }

        public class StringIntHandler : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class IntStringHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class EmptyHandlers
        {
            public virtual Task<Protobuf.Empty> One(Protobuf.Empty request, ServerCallContext context)
            {
                return Task.FromResult(new Protobuf.Empty());
            }

            public virtual Task<Protobuf.Empty> Two(Protobuf.Empty request, ServerCallContext context)
            {
                return Task.FromResult(new Protobuf.Empty());
            }
        }

        public class EmptyOneHandler : IHandler<Protobuf.Empty, Protobuf.Empty>
        {
            public Task<Protobuf.Empty> RunAsync(Protobuf.Empty request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class EmptyTwoHandler : IHandler<Protobuf.Empty, Protobuf.Empty>
        {
            public Task<Protobuf.Empty> RunAsync(Protobuf.Empty request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
