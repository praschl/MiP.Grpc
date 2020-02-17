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
using Proto = Google.Protobuf.WellKnownTypes;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class CommandQueryMethodGeneratorTest
    {
        private IMethodCodeGenerator _methodCodeGen;
        private IHandlerStore _handlerStore;
        private CommandQueryMethodGenerator _generator;

        [TestInitialize]
        public void Initialize()
        {
            _methodCodeGen = A.Fake<IMethodCodeGenerator>();
            _handlerStore = A.Fake<IHandlerStore>();

            _generator = new CommandQueryMethodGenerator(_methodCodeGen, _handlerStore);
        }

        [DataRow(nameof(MethodWithoutParams))]
        [DataRow(nameof(MethodWith1Params))]
        [DataRow(nameof(MethodWith3Params))]
        [DataRow(nameof(MethodWithWrongParamType))]
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

            A.CallTo(() => _handlerStore.Find(typeof(string), typeof(IHandler<string, int>), nameof(ValidMethodForHandler)))
                .Returns(expectedHandlerInfo);

            // act
            var result = _generator.Generate(typeof(string), methodInfo);

            // assert
            A.CallTo(() => _methodCodeGen.GenerateMethod(Code.MethodHandlerCode, nameof(ValidMethodForHandler), typeof(string), typeof(int), expectedHandlerInfo))
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
                .WithMessage("Couldn't find a type that implements *IHandler*[*String,*Int32] to handle method *Task*[*Int32] ValidMethodForHandler(*String, *ServerCallContext).")
                .And.Message.Should().NotContain("ICommandHandler");
        }

        [TestMethod]
        public void Generate_calls_generator_for_ICommandHandler_with_correct_parameters()
        {
            // arrange
            var methodInfo = GetMethodInfo(nameof(ValidMethodForCommandHandler));

            var expectedHandlerInfo = new HandlerInfo(null, null);

            A.CallTo(() => _handlerStore.Find(typeof(string), typeof(ICommandHandler<string>), nameof(ValidMethodForCommandHandler)))
                .Returns(expectedHandlerInfo);

            A.CallTo(() => _handlerStore.Find(typeof(string), typeof(IHandler<string, Proto.Empty>), nameof(ValidMethodForCommandHandler)))
                .Returns(null);

            // act
            var result = _generator.Generate(typeof(string), methodInfo);

            // assert
            A.CallTo(() => _methodCodeGen.GenerateMethod(Code.MethodCommandHandlerCode, nameof(ValidMethodForCommandHandler), typeof(string), typeof(Proto.Empty), expectedHandlerInfo))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void Generate_throws_when_no_Handler_for_ICommandHandler_found()
        {
            // arrange
            var methodInfo = GetMethodInfo(nameof(ValidMethodForCommandHandler));

            A.CallTo(() => _handlerStore.Find(null, null, null))
                .WithAnyArguments()
                .Returns(null);

            Action generate = () => _generator.Generate(typeof(string), methodInfo);

            generate.Should().Throw<InvalidOperationException>()
                .WithMessage("Couldn't find a type that implements *IHandler*[*String,*Empty] or *ICommandHandler*[*String] to handle method *Task*[*Empty] ValidMethodForCommandHandler(*String, *ServerCallContext).");
        }

        private MethodInfo GetMethodInfo(string name)
        {
            return typeof(CommandQueryMethodGeneratorTest)
                .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private void MethodWithoutParams() { }
        private void MethodWith1Params(int x) { }
        private void MethodWith3Params(int x, int y, int z) { }
        private void MethodWithWrongParamType(int request, long context) { }
        private List<int> MethodWithNoTaskInReturn(int request, ServerCallContext context) { return null; }
        private Task MethodWithNonGenericTaskInReturn(int request, ServerCallContext context) { return null; }

        private Task<int> ValidMethodForHandler(string request, ServerCallContext context)
        {
            return null;
        }

        private Task<Proto.Empty> ValidMethodForCommandHandler(string request, ServerCallContext context)
        {
            return null;
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}