using FakeItEasy;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class DispatcherCompiler_ClientStreaming_Test
    {
        private Guid _guid = Guid.NewGuid();

        private ServerCallContext _callContext;
        private IHandlerStore _handler;
        private IDispatcher _dispatcher;
        private MapGrpcServiceConfiguration _config;
        private DispatcherCompiler _compiler;

        [TestInitialize]
        public void Initialize()
        {
            _callContext = TestServerCallContext.Create(_guid.ToString(),
                null, DateTime.Now, null, CancellationToken.None, null, null, null, null, null, null);

            _handler = A.Fake<IHandlerStore>();
            _dispatcher = A.Fake<IDispatcher>();

            _config = new MapGrpcServiceConfiguration();

            _compiler = new DispatcherCompiler(_handler, _config);
        }

        [TestMethod]
        public async Task Execution_of_methods_for_IClientStreamHandler()
        {
            // arrange
            FakeHandler<string, int, ClientStreamHandler, ClientStreamingService>(nameof(ClientStreamingService.DoStream));

            var compiledType = _compiler.CompileDispatcher(typeof(ClientStreamingService));
            var service = Activator.CreateInstance(compiledType, _dispatcher) as ClientStreamingService;

            var fakeStream = A.Fake<IAsyncStreamReader<string>>();

            A.CallTo(() => _dispatcher.Dispatch<
             IAsyncStreamReader<string>,
             int,
             IClientStreamHandlerAdapter<string, int, ClientStreamHandler>>
             (
                 A<IAsyncStreamReader<string>>.That.Matches(ssr =>
                     ReferenceEquals(ssr, fakeStream)
                 ),
                 _callContext,
                 nameof(ClientStreamingService.DoStream))
             )
                .Returns(Task.FromResult(15));

            // act
            var result = await service.DoStream(fakeStream, _callContext).ConfigureAwait(false);

            // assert
            result.Should().Be(15);
        }

        private void FakeHandler<TRequest, TResponse, THandler, TServiceBase>(string methodName)
            where THandler : IClientStreamHandler<TRequest, TResponse>
        {
            A.CallTo(() => _handler.Find(typeof(TServiceBase), typeof(IClientStreamHandler<TRequest, TResponse>), methodName))
                .Returns(new HandlerInfo(typeof(THandler), null));
        }

        public class ClientStreamingService
        {
            public virtual Task<int> DoStream(IAsyncStreamReader<string> requestStream, ServerCallContext context)
            {
                return Task.FromResult(0);
            }
        }

        public class ClientStreamHandler : IClientStreamHandler<string, int>
        {
            public Task<int> RunAsync(IAsyncStreamReader<string> requestStream, ServerCallContext context)
            {
                return Task.FromResult(0);
            }
        }
    }
}
