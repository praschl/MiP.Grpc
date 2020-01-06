using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using MiP.Grpc.CodeGen;
using System;
using System.Reflection;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class MethodGeneratorTest
    {
        private IMethodCodeGenerator _methodCodeGen;

        [TestInitialize]
        public void Initialize()
        {
            _methodCodeGen = A.Fake<IMethodCodeGenerator>();
        }

        [TestMethod]
        public void Generate_calls_next_generator_when_CanGenerate_returns_false()
        {
            var generateCodeWasCalled = false;
            var nextWasCalled = false;

            var generator = new Testable(_methodCodeGen, (_) => false, (_, __) => { generateCodeWasCalled = true; return null; });
            var nextGenerator = new Testable(_methodCodeGen, (_) => { nextWasCalled = true; return false; });
            generator.NextGenerator = nextGenerator;

            var result = generator.Generate(null, null);

            // assert that GenerateCode was not called.
            generateCodeWasCalled.Should().BeFalse();
            nextWasCalled.Should().BeTrue();

            result.Should().BeNull();
        }

        [TestMethod]
        public void GenerateMethod_calls_MethodCodeGenerator()
        {
            // arrange
            var generator = new Testable(_methodCodeGen, (_) => true);

            generator.CodeTemplate = "<{Attributes}>-<{Method}>-<{Request}>-<{Response}>";
            generator.MethodName = "Method";
            generator.RequestType = typeof(int);
            generator.ResponseType = typeof(string);
            generator.HandlerInfo = new HandlerInfo(typeof(long), new[] { new AuthorizeAttribute("p.1") });

            var expectedResult = new GeneratedMethod(null, null, null, null);

            A.CallTo(() => _methodCodeGen.GenerateMethod(
                generator.CodeTemplate,
                generator.MethodName,
                generator.RequestType,
                generator.ResponseType, generator.HandlerInfo))
                .Returns(expectedResult);

            // act
            var result = generator.Generate(null, null);

            // assert
            result.Should().BeSameAs(expectedResult);
        }

        private class Testable : MethodGenerator
        {
            private readonly Func<MethodInfo, bool> _canGenerate;
            private readonly Func<Type, MethodInfo, GeneratedMethod> _generateCode;

            public Testable(IMethodCodeGenerator methodCodeGenerator, Func<MethodInfo, bool> canGenerate,
                Func<Type, MethodInfo, GeneratedMethod> generateCode = null
                )
                : base(methodCodeGenerator)
            {
                _canGenerate = canGenerate;
                _generateCode = generateCode;
            }

            protected override bool CanGenerate(MethodInfo method)
            {
                return _canGenerate(method);
            }

            public string CodeTemplate { get; set; }
            public string MethodName { get; set; }
            public Type RequestType { get; set; }
            public Type ResponseType { get; set; }
            public HandlerInfo HandlerInfo { get; set; }

            protected override GeneratedMethod GenerateCode(Type serviceBase, MethodInfo method)
            {
                if (_generateCode != null)
                    return _generateCode(serviceBase, method);

                return base.GenerateMethod(CodeTemplate, MethodName, RequestType, ResponseType, HandlerInfo);
            }
        }
    }
}