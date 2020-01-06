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
    public class ClientStreamMethodGeneratorTest
    {
        private IMethodCodeGenerator _methodCodeGen;
        private IHandlerStore _handlerStore;
        private ClientStreamMethodGenerator _generator;

        [TestInitialize]
        public void Initialize()
        {
            _methodCodeGen = A.Fake<IMethodCodeGenerator>();
            _handlerStore = A.Fake<IHandlerStore>();

            _generator = new ClientStreamMethodGenerator(_methodCodeGen, _handlerStore);
        }

        [DataRow(nameof(MethodWithoutParams))]
        [DataRow(nameof(MethodWith1Params))]
        [DataRow(nameof(MethodWith3Params))]
        [DataRow(nameof(MethodWrongRequestType))]
        [DataRow(nameof(MethodWrongRequestGenericType))]
        [DataRow(nameof(MethodWrongContextType))]
        [DataRow(nameof(MethodWithNoTaskInReturn))]
        [DataRow(nameof(MethodWithNonGenericTaskInReturn))]
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

            A.CallTo(() => _handlerStore.Find(typeof(string), typeof(IClientStreamHandler<int, string>), nameof(ValidMethodForHandler)))
                .Returns(expectedHandlerInfo);

            // act
            var result = _generator.Generate(typeof(string), methodInfo);

            // assert
            A.CallTo(() => _methodCodeGen.GenerateMethod(Code.MethodClientStreamHandlerCode, nameof(ValidMethodForHandler), typeof(int), typeof(string), expectedHandlerInfo))
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
                .WithMessage("Couldn't find a type that implements *IClientStreamHandler*[*Int32,*String] to handle method *Task*[*String] ValidMethodForHandler(*IAsyncStreamReader*[*Int32], *ServerCallContext).");
        }

        private MethodInfo GetMethodInfo(string name)
        {
            return typeof(ClientStreamMethodGeneratorTest)
                .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private void MethodWithoutParams() { }
        private void MethodWith1Params(int x) { }
        private void MethodWith3Params(int x, int y, int z) { }
        private void MethodWrongRequestType(int request, long context) { }
        private void MethodWrongRequestGenericType(List<int> request, long context) { }
        private void MethodWrongContextType(IAsyncStreamReader<int> request, long context) { }
        private List<int> MethodWithNoTaskInReturn(IAsyncStreamReader<int> request, ServerCallContext context) { return null; }
        private Task MethodWithNonGenericTaskInReturn(IAsyncStreamReader<int> request, ServerCallContext context) { return null; }

        private Task<string> ValidMethodForHandler(IAsyncStreamReader<int> request, ServerCallContext context) { return null; }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}