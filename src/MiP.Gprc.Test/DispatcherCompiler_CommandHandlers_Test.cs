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
    public class DispatcherCompiler_CommandHandlers_Test
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
        public void Adds_methods_for_ICommandHandler()
        {
            // arrange
            FakeCommandHandler<string, StringHandler, TwoHandlers>(nameof(TwoHandlers.One), null, null);
            FakeCommandHandler<int, IntHandler, TwoHandlers>(nameof(TwoHandlers.Two), 0, null);

            // act
            var result = _compiler.CompileDispatcher(typeof(TwoHandlers));

            // assert
            result.Should().HaveMethod(nameof(TwoHandlers.One), new[] { typeof(string), typeof(ServerCallContext) }).Which.ReturnType.Should().Be(typeof(Task<Protobuf.Empty>));
            result.Should().HaveMethod(nameof(TwoHandlers.Two), new[] { typeof(int), typeof(ServerCallContext) }).Which.ReturnType.Should().Be(typeof(Task<Protobuf.Empty>));
        }

        [TestMethod]
        public async Task Execution_of_methods_for_ICommandHandler()
        {
            // arrange
            FakeCommandHandler<string, StringHandler, TwoHandlers>(nameof(TwoHandlers.One), "request one", null);
            FakeCommandHandler<int, IntHandler, TwoHandlers>(nameof(TwoHandlers.Two), 2, null);

            var compiledType = _compiler.CompileDispatcher(typeof(TwoHandlers));
            var result = Activator.CreateInstance(compiledType, _dispatcher) as TwoHandlers;

            // act
            var executionResult1 = await result.One("request one", _callContext).ConfigureAwait(false);
            var executionResult2 = await result.Two(2, _callContext).ConfigureAwait(false);

            // assert
            executionResult1.Should().Be(new Protobuf.Empty());
            executionResult2.Should().Be(new Protobuf.Empty());
        }

        [TestMethod]
        public void Adds_methods_for_ICommandHandler_with_Empty()
        {
            // arrange
            FakeCommandHandler<Protobuf.Empty, EmptyOneHandler, EmptyHandlers>(nameof(EmptyHandlers.One), null, null);
            FakeCommandHandler<Protobuf.Empty, EmptyTwoHandler, EmptyHandlers>(nameof(EmptyHandlers.Two), null, null);

            // act
            var result = _compiler.CompileDispatcher(typeof(EmptyHandlers));

            // assert
            result.Should().HaveMethod(nameof(EmptyHandlers.One), new[] { typeof(Protobuf.Empty), typeof(ServerCallContext) }).Which.ReturnType.Should().Be(typeof(Task<Protobuf.Empty>));
            result.Should().HaveMethod(nameof(EmptyHandlers.Two), new[] { typeof(Protobuf.Empty), typeof(ServerCallContext) }).Which.ReturnType.Should().Be(typeof(Task<Protobuf.Empty>));
        }

        [TestMethod]
        public async Task Execution_of_methods_for_ICommandHandler_with_Empty()
        {
            // arrange
            FakeCommandHandler<Protobuf.Empty, EmptyOneHandler, EmptyHandlers>(nameof(EmptyHandlers.One), new Protobuf.Empty(), null);
            FakeCommandHandler<Protobuf.Empty, EmptyTwoHandler, EmptyHandlers>(nameof(EmptyHandlers.Two), new Protobuf.Empty(), null);

            var compiledType = _compiler.CompileDispatcher(typeof(EmptyHandlers));
            var result = Activator.CreateInstance(compiledType, _dispatcher) as EmptyHandlers;

            // act
            var executionResult1 = await result.One(new Protobuf.Empty(), _callContext).ConfigureAwait(false);
            var executionResult2 = await result.Two(new Protobuf.Empty(), _callContext).ConfigureAwait(false);

            // assert
            executionResult1.Should().Be(new Protobuf.Empty());
            executionResult2.Should().Be(new Protobuf.Empty());

            A.CallTo(() => _dispatcher.Dispatch<Protobuf.Empty, Protobuf.Empty, ICommandHandlerAdapter<Protobuf.Empty, EmptyOneHandler>>(new Protobuf.Empty(), _callContext, "One"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch<Protobuf.Empty, Protobuf.Empty, ICommandHandlerAdapter<Protobuf.Empty, EmptyTwoHandler>>(new Protobuf.Empty(), _callContext, "Two"))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void Adds_AuthorizeAttribute_to_ICommandHandler_methods()
        {
            // arrange
            FakeCommandHandler<string, StringHandler, TwoHandlers>(nameof(TwoHandlers.One), null, _methodOneAttributes);
            FakeCommandHandler<int, IntHandler, TwoHandlers>(nameof(TwoHandlers.Two), 0, _methodTwoAttributes);

            // act
            var result = _compiler.CompileDispatcher(typeof(TwoHandlers));

            // assert
            result.GetMethod(nameof(TwoHandlers.One)).GetCustomAttributes<AuthorizeAttribute>().Should().BeEquivalentTo(_methodOneAttributes);
            result.GetMethod(nameof(TwoHandlers.Two)).GetCustomAttributes<AuthorizeAttribute>().Should().BeEquivalentTo(_methodTwoAttributes);
        }

        private void FakeCommandHandler<TCommand, THandler, TServiceBase>(string methodName, TCommand command, IReadOnlyCollection<AuthorizeAttribute> attributes = null)
            where THandler : ICommandHandler<TCommand>
        {
            attributes ??= new AuthorizeAttribute[0];

            // first return null for the IHandler
            A.CallTo(() => _handler.Find(typeof(TServiceBase), typeof(IHandler<TCommand, Protobuf.Empty>), methodName))
                .Returns(null);

            A.CallTo(() => _handler.Find(typeof(TServiceBase), typeof(ICommandHandler<TCommand>), methodName))
                .Returns(new HandlerInfo(typeof(THandler), attributes));

            A.CallTo(() => _dispatcher.Dispatch<TCommand, Protobuf.Empty, ICommandHandlerAdapter<TCommand, THandler>>(command, _callContext, methodName))
                .Returns(Task.FromResult(new Protobuf.Empty()));
        }

        public class TwoHandlers
        {
            public virtual Task<Protobuf.Empty> One(string request, ServerCallContext context)
            {
                return Task.FromResult(new Protobuf.Empty());
            }

            public virtual Task<Protobuf.Empty> Two(int request, ServerCallContext context)
            {
                return Task.FromResult(new Protobuf.Empty());
            }
        }

        public class StringHandler : ICommandHandler<string>
        {
            public Task RunAsync(string command, ServerCallContext context)
            {
                return Task.CompletedTask;
            }
        }

        public class IntHandler : ICommandHandler<int>
        {
            public Task RunAsync(int command, ServerCallContext context)
            {
                return Task.CompletedTask;
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

        public class EmptyOneHandler : ICommandHandler<Protobuf.Empty>
        {
            public Task RunAsync(Protobuf.Empty command, ServerCallContext context)
            {
                return Task.CompletedTask;
            }
        }

        public class EmptyTwoHandler : ICommandHandler<Protobuf.Empty>
        {
            public Task RunAsync(Protobuf.Empty command, ServerCallContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
