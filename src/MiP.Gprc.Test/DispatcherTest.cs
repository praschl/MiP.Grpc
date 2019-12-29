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
    public class DispatcherTest
    {
        private IServiceProvider _serviceProvider;
        private Dispatcher _dispatcher;

        [TestInitialize]
        public void Initialize()
        {
            _serviceProvider = A.Fake<IServiceProvider>();
            _dispatcher = new Dispatcher(_serviceProvider);
        }

        [TestMethod]
        public async Task Dispatch_calls_handlers_RunAsync_and_returns_result()
        {
            // arrange
            var callContext = TestServerCallContext.Create(nameof(Dispatch_calls_handlers_RunAsync_and_returns_result),
                null, DateTime.Now, null, CancellationToken.None, null, null, null, null, null, null);

            A.CallTo(() => _serviceProvider.GetService(typeof(IntStringHandler))).Returns(new IntStringHandler());

            // expected
            const string expected = nameof(Dispatch_calls_handlers_RunAsync_and_returns_result) + " 1234";

            // act
            var result = await _dispatcher.Dispatch<int, string, IntStringHandler>(1234, callContext, "IntString");

            // assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void Dispatch_fails_when_ServiceProvider_returns_null()
        {
            // arrange
            A.CallTo(() => _serviceProvider.GetService(typeof(IntStringHandler))).Returns(null);

            // act
            Func<Task> dispatch = async () => await _dispatcher.Dispatch<int, string, IntStringHandler>(1, null, null);

            // assert
            dispatch.Should().ThrowExactly<InvalidOperationException>().WithMessage($"No service for type '{typeof(IntStringHandler).FullName}' has been registered.");
        }

        public class IntStringHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                return Task.FromResult(context.Method + " " + request);
            }
        }
    }
}
