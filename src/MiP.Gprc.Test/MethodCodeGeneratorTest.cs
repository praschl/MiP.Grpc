using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using MiP.Grpc.CodeGen;
using System;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class MethodCodeGeneratorTest
    {
        private MethodCodeGenerator _generator;

        [TestInitialize]
        public void Initialize()
        {
            _generator = new MethodCodeGenerator();
        }

        [TestMethod]
        public void GenerateMethod_generates_method_with_attributes()
        {
            var template = @"{Attributes}-
{Method}-
{Request}-
{Response}-
{Handler}";

            var handlerInfo = new HandlerInfo(typeof(long), new[] { new AuthorizeAttribute { Roles = "role.1" } });

            var expectedCode = RemoveSpaces(@"
[global::Microsoft.AspNetCore.Authorization.Authorize(Roles = ""role.1"")]-
method.1-
global::System.Int32-
global::System.String-
global::System.Int64");

            var expected = new GeneratedMethod(typeof(long), typeof(int), typeof(string), expectedCode);

            var result = _generator.GenerateMethod(template, "method.1", typeof(int), typeof(string), handlerInfo);

            result.Should().BeEquivalentTo(expected, o => o.Excluding(x => x.Code));

            var resultCode = RemoveSpaces(result.Code);
            resultCode.Should().Be(expectedCode);
        }

        private string RemoveSpaces(string value)
        {
            return value.Replace(" ", "").Replace(Environment.NewLine, "");
        }
    }
}