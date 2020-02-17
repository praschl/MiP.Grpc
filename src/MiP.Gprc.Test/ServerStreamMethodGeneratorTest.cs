using FakeItEasy;
using FluentAssertions;
using Grpc.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using MiP.Grpc.CodeGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class ServerStreamMethodGeneratorTest
    {
        private IMethodCodeGenerator _methodCodeGen;
        private IHandlerStore _handlerStore;
        private ServerStreamMethodGenerator _generator;

        [TestInitialize]
        public void Initialize()
        {
            _methodCodeGen = A.Fake<IMethodCodeGenerator>();
            _handlerStore = A.Fake<IHandlerStore>();

            _generator = new ServerStreamMethodGenerator(_methodCodeGen, _handlerStore);
        }

        [DataRow(nameof(MethodWith2Params))]
        [DataRow(nameof(MethodWith4Params))]
        [DataRow(nameof(MethodWithWrongParamTypeForContext))]
        [DataRow(nameof(MethodWithWrongReturnType))]
        [DataRow(nameof(MethodWithWrongParamTypeForStream))]
        [DataRow(nameof(MethodWithWrongGenericTypeForStream))]
        [DataTestMethod]
        public void Generate_returns_null(string methodName)
        {
            var methodInfo = GetMethodInfo(methodName);

            var result = _generator.Generate(typeof(string), methodInfo);

            result.Should().BeNull();
        }

        [TestMethod]
        public void Generate_calls_generator_for_IHandler_with_correct_parameters()
        {
            // arrange
            var methodInfo = GetMethodInfo(nameof(ValidMethodForHandler));

            var expectedHandlerInfo = new HandlerInfo(null, null);

            A.CallTo(() => _handlerStore.Find(typeof(string), typeof(IServerStreamHandler<int, string>), nameof(ValidMethodForHandler)))
                .Returns(expectedHandlerInfo);

            // act
            var result = _generator.Generate(typeof(string), methodInfo);

            // assert
            A.CallTo(() => _methodCodeGen.GenerateMethod(Code.MethodServerStreamHandlerCode, nameof(ValidMethodForHandler), typeof(int), typeof(string), expectedHandlerInfo))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void Generate_throws_when_no_Handler_for_IHandler_found()
        {
            // arrange
            var methodInfo = GetMethodInfo(nameof(ValidMethodForHandler));

            A.CallTo(() => _handlerStore.Find(null, null, null))
                .WithAnyArguments()
                .Returns(null);

            Action generate = () => _generator.Generate(typeof(string), methodInfo);

            generate.Should().Throw<InvalidOperationException>()
                .WithMessage("Couldn't find a type that implements *IServerStreamHandler*[*Int32,*String] to handle method *Task ValidMethodForHandler(Int32, *IServerStreamWriter*[*String], *ServerCallContext).");
        }

        private MethodInfo GetMethodInfo(string name)
        {
            return typeof(ServerStreamMethodGeneratorTest)
                .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private void MethodWith2Params(int x, int y) { }
        private void MethodWith4Params(int x, int y, int z, int a) { }
        private void MethodWithWrongParamTypeForContext(int request, IServerStreamWriter<long> serverStreamWriter, long context) { }
        private void MethodWithWrongReturnType(int request, IServerStreamWriter<long> serverStreamWriter, ServerCallContext context) { }
        private Task MethodWithWrongParamTypeForStream(int request, long serverStreamWriter, ServerCallContext context)
        {
            return null;
        }

        private Task MethodWithWrongGenericTypeForStream(int request, List<long> serverStreamWriter, ServerCallContext context)
        {
            return null;
        }

        private Task ValidMethodForHandler(int request, IServerStreamWriter<string> serverStreamWriter, ServerCallContext context)
        {
            return null;
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }

    [TestClass]
    public class BidiStreamMethodGeneratorTest
    {
        private IMethodCodeGenerator _methodCodeGen;
        private IHandlerStore _handlerStore;
        private BidiStreamMethodGenerator _generator;

        [TestInitialize]
        public void Initialize()
        {
            _methodCodeGen = A.Fake<IMethodCodeGenerator>();
            _handlerStore = A.Fake<IHandlerStore>();

            _generator = new BidiStreamMethodGenerator(_methodCodeGen, _handlerStore);
        }

        [DataRow(nameof(MethodWith2Params))]
        [DataRow(nameof(MethodWith4Params))]
        [DataRow(nameof(MethodWithWrongParamTypeForContext))]
        [DataRow(nameof(MethodWithWrongReturnType))]
        [DataRow(nameof(MethodWithWrongParamTypeForRequestStream))]
        [DataRow(nameof(MethodWithWrongGenericTypeForRequestStream))]
        [DataRow(nameof(MethodWithWrongParamTypeForResponseStream))]
        [DataRow(nameof(MethodWithWrongGenericTypeForResponseStream))]
        [DataTestMethod]
        public void Generate_returns_null(string methodName)
        {
            var methodInfo = GetMethodInfo(methodName);

            var result = _generator.Generate(typeof(string), methodInfo);

            result.Should().BeNull();
        }

        [TestMethod]
        public void Generate_calls_generator_for_IHandler_with_correct_parameters()
        {
            // arrange
            var methodInfo = GetMethodInfo(nameof(ValidMethodForHandler));

            var expectedHandlerInfo = new HandlerInfo(null, null);

            A.CallTo(() => _handlerStore.Find(typeof(string), typeof(IBidiStreamHandler<int, string>), nameof(ValidMethodForHandler)))
                .Returns(expectedHandlerInfo);

            // act
            var result = _generator.Generate(typeof(string), methodInfo);

            // assert
            A.CallTo(() => _methodCodeGen.GenerateMethod(Code.MethodBidiStreamHandlerCode, nameof(ValidMethodForHandler), typeof(int), typeof(string), expectedHandlerInfo))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void Generate_throws_when_no_Handler_for_IHandler_found()
        {
            // arrange
            var methodInfo = GetMethodInfo(nameof(ValidMethodForHandler));

            A.CallTo(() => _handlerStore.Find(null, null, null))
                .WithAnyArguments()
                .Returns(null);

            Action generate = () => _generator.Generate(typeof(string), methodInfo);

            generate.Should().Throw<InvalidOperationException>()
                .WithMessage("Couldn't find a type that implements *IBidiStreamHandler*[*Int32,*String] to handle method *Task ValidMethodForHandler(*IAsyncStreamReader*[*Int32], *IServerStreamWriter*[*String], *ServerCallContext).");
        }

        private MethodInfo GetMethodInfo(string name)
        {
            return typeof(BidiStreamMethodGeneratorTest)
                .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private void MethodWith2Params(int x, int y) { }
        private void MethodWith4Params(int x, int y, int z, int a) { }
        private void MethodWithWrongParamTypeForContext(IAsyncStreamReader<long> requestStreamReader, IServerStreamWriter<long> serverStreamWriter, long context) { }
        private void MethodWithWrongReturnType(IAsyncStreamReader<long> requestStreamReader, IServerStreamWriter<long> serverStreamWriter, ServerCallContext context) { }
        private Task MethodWithWrongParamTypeForRequestStream(long requestStreamReader, IServerStreamWriter<long> serverStreamWriter, ServerCallContext context)
        {
            return null;
        }

        private Task MethodWithWrongGenericTypeForRequestStream(List<long> requestStreamReader, IServerStreamWriter<long> serverStreamWriter, ServerCallContext context)
        {
            return null;
        }

        private Task MethodWithWrongParamTypeForResponseStream(IAsyncStreamReader<long> requestStreamReader, long serverStreamWriter, ServerCallContext context)
        {
            return null;
        }

        private Task MethodWithWrongGenericTypeForResponseStream(IAsyncStreamReader<long> requestStreamReader, List<long> serverStreamWriter, ServerCallContext context)
        {
            return null;
        }

        private Task ValidMethodForHandler(IAsyncStreamReader<int> requestStreamReader, IServerStreamWriter<string> serverStreamWriter, ServerCallContext context)
        {
            return null;
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}