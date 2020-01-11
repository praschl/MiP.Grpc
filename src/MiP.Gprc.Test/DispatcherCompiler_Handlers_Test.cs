using FakeItEasy;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Protobuf = Google.Protobuf.WellKnownTypes;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class DispatcherCompiler_Handlers_Test
    {
        private Guid _guid = Guid.NewGuid();

        private ServerCallContext _callContext;
        private AuthorizeAttribute[] _methodOneAttributes;
        private AuthorizeAttribute[] _methodTwoAttributes;
        private IHandlerStore _handler;
        private IDispatcher _dispatcher;
        private MapGrpcServiceConfiguration _config;
        private DispatcherCompiler _compiler;

        [TestInitialize]
        public void Initialize()
        {
            _callContext = TestServerCallContext.Create(_guid.ToString(),
                null, DateTime.Now, null, CancellationToken.None, null, null, null, null, null, null);

            _methodOneAttributes = new[]
            {
                new AuthorizeAttribute("my.policy.1") { Roles = "my.role.1", AuthenticationSchemes = "my.scheme.1" },
                new AuthorizeAttribute("my.policy.2") { Roles = "my.role.2", AuthenticationSchemes = "my.scheme.2" },
            };

            _methodTwoAttributes = new[]
            {
                new AuthorizeAttribute("my.policy.3") { Roles = "my.role.3", AuthenticationSchemes = "my.scheme.3" },
                new AuthorizeAttribute("my.policy.4") { Roles = "my.role.4", AuthenticationSchemes = "my.scheme.4" },
            };

            _handler = A.Fake<IHandlerStore>();
            _dispatcher = A.Fake<IDispatcher>();

            _config = new MapGrpcServiceConfiguration();

            _compiler = new DispatcherCompiler(_handler, _config);
        }

        [TestMethod]
        public void Adds_methods_for_IHandler()
        {
            // arrange
            FakeHandler<string, int, StringIntHandler, TwoHandlers>(nameof(TwoHandlers.One), null, 0);
            FakeHandler<int, string, IntStringHandler, TwoHandlers>(nameof(TwoHandlers.Two), 0, null);

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
            FakeHandler<string, int, StringIntHandler, TwoHandlers>(nameof(TwoHandlers.One), "request one", 14);
            FakeHandler<int, string, IntStringHandler, TwoHandlers>(nameof(TwoHandlers.Two), 2, "response two");

            var compiledType = _compiler.CompileDispatcher(typeof(TwoHandlers));
            var result = Activator.CreateInstance(compiledType, _dispatcher) as TwoHandlers;

            // act
            var executionResult1 = await result.One("request one", _callContext).ConfigureAwait(false);
            var executionResult2 = await result.Two(2, _callContext).ConfigureAwait(false);

            // assert
            executionResult1.Should().Be(14);
            executionResult2.Should().Be("response two");
        }

        [TestMethod]
        public void Adds_methods_for_IHandler_with_Empty()
        {
            // arrange
            FakeHandler<Protobuf.Empty, Protobuf.Empty, EmptyOneHandler, EmptyHandlers>(nameof(EmptyHandlers.One), null, null);
            FakeHandler<Protobuf.Empty, Protobuf.Empty, EmptyTwoHandler, EmptyHandlers>(nameof(EmptyHandlers.Two), null, null);

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
            FakeHandler<Protobuf.Empty, Protobuf.Empty, EmptyOneHandler, EmptyHandlers>(nameof(EmptyHandlers.One), new Protobuf.Empty(), new Protobuf.Empty());
            FakeHandler<Protobuf.Empty, Protobuf.Empty, EmptyTwoHandler, EmptyHandlers>(nameof(EmptyHandlers.Two), new Protobuf.Empty(), new Protobuf.Empty());

            var compiledType = _compiler.CompileDispatcher(typeof(EmptyHandlers));
            var result = Activator.CreateInstance(compiledType, _dispatcher) as EmptyHandlers;

            // act
            var executionResult1 = await result.One(new Protobuf.Empty(), _callContext).ConfigureAwait(false);
            var executionResult2 = await result.Two(new Protobuf.Empty(), _callContext).ConfigureAwait(false);

            // assert
            executionResult1.Should().Be(new Protobuf.Empty());
            executionResult2.Should().Be(new Protobuf.Empty());

            A.CallTo(() => _dispatcher.Dispatch<Protobuf.Empty, Protobuf.Empty, EmptyOneHandler>(new Protobuf.Empty(), _callContext, "One"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch<Protobuf.Empty, Protobuf.Empty, EmptyTwoHandler>(new Protobuf.Empty(), _callContext, "Two"))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void Adds_AuthorizeAttribute_to_IHandler_methods()
        {
            // arrange
            FakeHandler<string, int, StringIntHandler, TwoHandlers>(nameof(TwoHandlers.One), null, 0, _methodOneAttributes);
            FakeHandler<int, string, IntStringHandler, TwoHandlers>(nameof(TwoHandlers.Two), 0, null, _methodTwoAttributes);

            // act
            var result = _compiler.CompileDispatcher(typeof(TwoHandlers));

            // assert
            result.GetMethod(nameof(TwoHandlers.One)).GetCustomAttributes<AuthorizeAttribute>().Should().BeEquivalentTo(_methodOneAttributes);
            result.GetMethod(nameof(TwoHandlers.Two)).GetCustomAttributes<AuthorizeAttribute>().Should().BeEquivalentTo(_methodTwoAttributes);
        }

        private void FakeHandler<TRequest, TResponse, THandler, TServiceBase>(string methodName, TRequest request, TResponse response, IReadOnlyCollection<AuthorizeAttribute> attributes = null)
            where THandler : IHandler<TRequest, TResponse>
        {
            attributes ??= new AuthorizeAttribute[0];

            A.CallTo(() => _handler.FindHandlerMap(methodName, typeof(TRequest), typeof(TResponse), typeof(TServiceBase)))
                .Returns(new DispatcherMap(new DispatcherMapKey(methodName, typeof(TRequest), typeof(TResponse), typeof(TServiceBase)), typeof(THandler), attributes));

            A.CallTo(() => _dispatcher.Dispatch<TRequest, TResponse, THandler>(request, _callContext, methodName))
            .Returns(Task.FromResult(response));
        }

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
                return Task.FromResult(0);
            }
        }

        public class IntStringHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                return Task.FromResult(string.Empty);
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
                return Task.FromResult(new Protobuf.Empty());
            }
        }

        public class EmptyTwoHandler : IHandler<Protobuf.Empty, Protobuf.Empty>
        {
            public Task<Protobuf.Empty> RunAsync(Protobuf.Empty request, ServerCallContext context)
            {
                return Task.FromResult(new Protobuf.Empty());
            }
        }
    }
}
