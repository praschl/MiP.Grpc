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
    public class DispatcherCompiler_BidiStreaming_Test
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
        /// Obsolete to avoid warningfrom the <see cref="ServerStreamRequest{TRequest, TStream}"/> which is marked as obsolete for internal use.
        /// </summary>
        [TestMethod]
        [Obsolete]
        public async Task Execution_of_methods_for_IBidiStreamHandler()
        {
            // arrange
            FakeHandler<string, int, BidiStreamHandler, BidiStreamingService>(nameof(BidiStreamingService.DoStream));

            var compiledType = _compiler.CompileDispatcher(typeof(BidiStreamingService));
            var service = Activator.CreateInstance(compiledType, _dispatcher) as BidiStreamingService;

            var fakeRequestStream = A.Fake<IAsyncStreamReader<string>>();
            var fakeResponseStream = A.Fake<IServerStreamWriter<int>>();

            // act
            await service.DoStream(fakeRequestStream, fakeResponseStream, _callContext).ConfigureAwait(false);

            // assert
            A.CallTo(() => _dispatcher.Dispatch<
                IBidiStreamRequest<string, int>,
                Protobuf.Empty,
                IBidiStreamHandlerAdapter<string, int, BidiStreamHandler>>
                (
                    A<IBidiStreamRequest<string, int>>.That.Matches(ssr =>
                        ReferenceEquals(ssr.RequestStream, fakeRequestStream)
                        &&
                        ReferenceEquals(ssr.ResponseStream, fakeResponseStream)
                        &&
                        ssr.GetType().GetGenericTypeDefinition() == typeof(BidiStreamRequest<,>)
                    ),
                    _callContext,
                    nameof(BidiStreamingService.DoStream))
                )
                .MustHaveHappened();
        }

        private void FakeHandler<TRequest, TResponse, THandler, TServiceBase>(string methodName)
            where THandler : IBidiStreamHandler<TRequest, TResponse>
        {
            A.CallTo(() => _handler.Find(typeof(TServiceBase), typeof(IBidiStreamHandler<TRequest, TResponse>), methodName))
                .Returns(new HandlerInfo(typeof(THandler), null));
        }

        public class BidiStreamingService
        {
            public virtual Task DoStream(IAsyncStreamReader<string> requestStream, IServerStreamWriter<int> responseStream, ServerCallContext context)
            {
                return Task.FromResult(0);
            }
        }

        public class BidiStreamHandler : IBidiStreamHandler<string, int>
        {
            public Task RunAsync(IBidiStreamRequest<string, int> streamRequest, ServerCallContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
