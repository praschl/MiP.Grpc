using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using System.Reflection;

namespace MiP.Gprc.Test
{
    [TestClass]
    public class DispatcherCompiler_Class_And_Constructor_Test
    {
        private IHandlerStore _handler;
        private MapGrpcServiceConfiguration _config;
        private DispatcherCompiler _compiler;

        [TestInitialize]
        public void Initialize()
        {
            _handler = A.Fake<IHandlerStore>();

            _config = new MapGrpcServiceConfiguration();

            _compiler = new DispatcherCompiler(_handler, _config);
        }

        [TestMethod]
        public void Compiles_class_with_constructor()
        {
            // act
            var result = _compiler.CompileDispatcher(typeof(EmptyBase));

            // assert
            result.Should().NotBeNull();
            result.Should().HaveConstructor(new[] { typeof(IDispatcher) });
        }

        [TestMethod]
        public void Adds_AuthorizeAttributes_to_class()
        {
            // arrange
            _config.AddGlobalAuthorizeAttribute("my.policy.1", "my.roles.1", "my.scheme.1");
            _config.AddGlobalAuthorizeAttribute("my.policy.2", "my.roles.2", "my.scheme.2");

            // expected
            var authorize1 = new AuthorizeAttribute("my.policy.1") { Roles = "my.roles.1", AuthenticationSchemes = "my.scheme.1" };
            var authorize2 = new AuthorizeAttribute("my.policy.2") { Roles = "my.roles.2", AuthenticationSchemes = "my.scheme.2" };

            // act
            var result = _compiler.CompileDispatcher(typeof(EmptyBase));

            // assert
            var attributes = result.GetCustomAttributes<AuthorizeAttribute>();
            attributes.Should().BeEquivalentTo(new[] { authorize1, authorize2 });

            result.Should().NotBe<EmptyBase>();
            result.Should().BeAssignableTo<EmptyBase>();
        }

        public class EmptyBase { }
    }
}
