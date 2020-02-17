using FakeItEasy;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using MiP.Grpc.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;
using Protobuf = Google.Protobuf.WellKnownTypes;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class DispatcherCompiler_ServerStreaming_Test
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

        /// <summary>
        /// Obsolete to avoid warning from the <see cref="ServerStreamRequest{TRequest, TStream}"/> which is marked as obsolete for internal use.
        /// </summary>
        [TestMethod]
        [Obsolete]
        public async Task Execution_of_methods_for_IServerStreamHandler()
        {
            // arrange
            FakeHandler<string, int, ServerStreamHandler, ServerStreamingService>(nameof(ServerStreamingService.DoStream));

            var compiledType = _compiler.CompileDispatcher(typeof(ServerStreamingService));
            var service = Activator.CreateInstance(compiledType, _dispatcher) as ServerStreamingService;

            var fakeStream = A.Fake<IServerStreamWriter<int>>();

            // act
            await service.DoStream("request one", fakeStream, _callContext).ConfigureAwait(false);

            // assert
            A.CallTo(() => _dispatcher.Dispatch<
                IServerStreamRequest<string, int>,
                Protobuf.Empty,
                IServerStreamHandlerAdapter<string, int, ServerStreamHandler>>
                (
                    A<IServerStreamRequest<string, int>>.That.Matches(ssr =>
                        ssr.Request == "request one"
                        &&
                        ReferenceEquals(ssr.Stream, fakeStream)
                        &&
                        ssr.GetType().GetGenericTypeDefinition() == typeof(ServerStreamRequest<,>)
                    ),
                    _callContext,
                    nameof(ServerStreamingService.DoStream))
                )
                .MustHaveHappened();
        }

        private void FakeHandler<TRequest, TResponse, THandler, TServiceBase>(string methodName)
            where THandler : IServerStreamHandler<TRequest, TResponse>
        {
            A.CallTo(() => _handler.Find(typeof(TServiceBase), typeof(IServerStreamHandler<TRequest, TResponse>), methodName))
                .Returns(new HandlerInfo(typeof(THandler), null));
        }

        public class ServerStreamingService
        {
            public virtual Task DoStream(string request, IServerStreamWriter<int> responseStream, ServerCallContext context)
            {
                return Task.FromResult(0);
            }
        }

        public class ServerStreamHandler : IServerStreamHandler<string, int>
        {
            public Task RunAsync(IServerStreamRequest<string, int> streamRequest, ServerCallContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
