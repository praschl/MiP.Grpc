using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc.CodeGen;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class AttributeCodeGeneratorTest
    {
        [TestMethod]
        public void Generates_one_Attribute()
        {
            var attributes = new[]
            {
                new AuthorizeAttribute{ AuthenticationSchemes = "sch.1", Policy = "p.1", Roles = "r.1" }
            };

            var result = AttributeCodeGenerator.GenerateAttributes(attributes);

            result.Should().Contain("[global::Microsoft.AspNetCore.Authorization.Authorize(Roles = \"r.1\", Policy = \"p.1\", AuthenticationSchemes = \"sch.1\")]");
        }

        [TestMethod]
        public void Generates_two_Attributes()
        {
            var attributes = new[]
            {
                new AuthorizeAttribute{ AuthenticationSchemes = "sch.1", Policy = "p.1", Roles = "r.1" },
                new AuthorizeAttribute{ AuthenticationSchemes = "sch.2", Policy = "p.2", Roles = "r.2" }
            };

            var result = AttributeCodeGenerator.GenerateAttributes(attributes);

            result.Should().Contain("[global::Microsoft.AspNetCore.Authorization.Authorize(Roles = \"r.1\", Policy = \"p.1\", AuthenticationSchemes = \"sch.1\")]");
            result.Should().Contain("[global::Microsoft.AspNetCore.Authorization.Authorize(Roles = \"r.2\", Policy = \"p.2\", AuthenticationSchemes = \"sch.2\")]");
        }
    }
}