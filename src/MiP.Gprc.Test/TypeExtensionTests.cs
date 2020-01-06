using FluentAssertions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiP.Grpc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MiP.Gprc.Test
{
    [SuppressMessage("General", "RCS1079:Throwing of new NotImplementedException.", Justification = "Test classes")]
    [TestClass]
    public class TypeExtensionTests
    {
        [DataRow(typeof(IHandler<string, int>), true)]
        [DataRow(typeof(ICommandHandler<string>), true)]
        [DataRow(typeof(IClientStreamHandler<string, int>), true)]
        [DataRow(typeof(IServerStreamHandler<string, int>), true)]
        [DataRow(typeof(IBidiStreamHandler<string, int>), true)]
        [DataRow(typeof(IHandler<,>), false)]
        [DataRow(typeof(ICommandHandler<>), false)]
        [DataRow(typeof(IClientStreamHandler<,>), false)]
        [DataRow(typeof(IServerStreamHandler<,>), false)]
        [DataRow(typeof(IBidiStreamHandler<,>), false)]
        [DataRow(typeof(IDisposable), false)]
        [DataTestMethod]
        public void IsHandlerInterface(Type candidate, bool isHandlerInterface)
        {
            candidate.IsHandlerInterface().Should().Be(isHandlerInterface);
        }

        [DataRow(typeof(HandlerImplementsOne), 1)]
        [DataRow(typeof(HandlerImplementsThree), 3)]
        [DataRow(typeof(MemoryStream), 0)]
        [DataTestMethod]
        public void GetHandlerInterfaces(Type candidate, int count)
        {
            candidate.GetHandlerInterfaces().Count().Should().Be(count);
        }

        [DataRow(typeof(HandlerImplementsOne), true)]
        [DataRow(typeof(HandlerImplementsThree), true)]
        [DataRow(typeof(MemoryStream), false)]
        [DataTestMethod]
        public void HasHandlerInterface(Type candidate, bool hasHandler)
        {
            candidate.HasHandlerInterface().Should().Be(hasHandler);
        }

        public class HandlerImplementsOne : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class HandlerImplementsThree : IHandler<string, int>, IHandler<int, string>, ICommandHandler<int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }

            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }

            Task ICommandHandler<int>.RunAsync(int command, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The Test Method for the overload with two arguments must have a different name, or it will not be found.
        /// </summary>
        [DataRow(typeof(HandlerWithAttributes), typeof(IHandler<string, int>))]
        [DataRow(typeof(HandlerWithAttributes), typeof(ICommandHandler<string>))]
        [DataTestMethod]
        public void GetHandlesAttribute2(Type handler, Type iface)
        {
            var expected = new HandlesAttribute("Method.Name", typeof(IDisposable));

            handler.GetHandlesAttribute(iface).Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetHandlesAttribute2_when_no_Attribute()
        {
            typeof(HandlerWithAuthorize)
                .GetHandlesAttribute(typeof(IHandler<string, int>))
                .Should().BeNull();
        }

        /// <summary>
        /// The Test Method for the overload with one argument must have a different name, or it will not be found.
        /// </summary>
        [DataRow(typeof(HandlerWithAttributes))]
        [DataTestMethod]
        public void GetHandlesAttribute1(Type handler)
        {
            var expected = new HandlesAttribute("On.Method", typeof(IDisposable));

            handler.GetHandlesAttribute().Should().BeEquivalentTo(expected);
        }

        [Handles("On.Method", typeof(IDisposable))]
        public class HandlerWithAttributes : IHandler<string, int>, ICommandHandler<string>
        {
            [Handles("Method.Name", typeof(IDisposable))]
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }

            [Handles("Method.Name", typeof(IDisposable))]
            Task ICommandHandler<string>.RunAsync(string command, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [DataRow(typeof(HandlerWithAuthorize), typeof(IHandler<string, int>))]
        [DataRow(typeof(HandlerWithAuthorize), typeof(ICommandHandler<string>))]
        [DataTestMethod]
        public void GetAuthorizeAttributes(Type handler, Type iface)
        {
            AuthorizeAttribute[] expected =
            {
                new AuthorizeAttribute { Roles = "on.class.1" },
                new AuthorizeAttribute { Roles = "on.class.2" },
                new AuthorizeAttribute { Roles = "on.method.1" },
                new AuthorizeAttribute { Roles = "on.method.2" }
            };

            handler.GetAuthorizeAttributes(iface).Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetAuthorizeAttributes_when_no_Attribute()
        {
            typeof(HandlerWithAttributes)
                .GetAuthorizeAttributes(typeof(IHandler<string, int>))
                .Should()
                .BeEmpty();
        }

        [Authorize(Roles = "on.class.1")]
        [Authorize(Roles = "on.class.2")]
        public class HandlerWithAuthorize : IHandler<string, int>, ICommandHandler<string>
        {
            [Authorize(Roles = "on.method.1")]
            [Authorize(Roles = "on.method.2")]
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }

            [Authorize(Roles = "on.method.1")]
            [Authorize(Roles = "on.method.2")]
            Task ICommandHandler<string>.RunAsync(string command, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [DataRow(typeof(MemoryStream), typeof(IHandler<,>), "MemoryStream")]
        [DataRow(typeof(NameOneHandler), typeof(IHandler<string, int>), "NameOne")]
        [DataRow(typeof(NameTwoCommandHandler), typeof(ICommandHandler<string>), "NameTwo")]
        [DataTestMethod]
        public void GetHandlerNameFromClass(Type clazz, Type iface, string expectedName)
        {
            clazz.GetHandlerNameFromClass(iface).Should().Be(expectedName);
        }

        public class NameOneHandler : IHandler<int, string>
        {
            public Task<string> RunAsync(int request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class NameTwoCommandHandler : ICommandHandler<string>
        {
            public Task RunAsync(string command, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [DataRow(typeof(NameFromMethod), typeof(IHandler<string, int>), "Name.From.Method")]
        [DataRow(typeof(NameFromClassAttribute), typeof(IHandler<string, int>), "Name.From.Class.Attribute")]
        [DataRow(typeof(NameFromClassHandler), typeof(IHandler<string, int>), "NameFromClass")]
        [DataTestMethod]
        public void GetMethodName(Type handler, Type iface, string expected)
        {
            handler.GetMethodName(iface).Should().Be(expected);
        }

        public class NameFromMethod : IHandler<string, int>
        {
            [Handles("Name.From.Method")]
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Handles("Name.From.Class.Attribute")]
        public class NameFromClassAttribute : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class NameFromClassHandler : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [DataRow(typeof(ServiceBaseFromMethod), typeof(IHandler<string, int>), typeof(IDisposable))]
        [DataRow(typeof(ServiceBaseFromClass), typeof(IHandler<string, int>), typeof(IDisposable))]
        [DataRow(typeof(WithoutServiceBase), typeof(IHandler<string, int>), null)]
        [DataTestMethod]
        public void GetServiceBase(Type handler, Type iface, Type expected)
        {
            handler.GetServiceBase(iface).Should().Be(expected);
        }

        [Handles(ServiceBase = typeof(MemoryStream))]
        public class ServiceBaseFromMethod : IHandler<string, int>
        {
            [Handles(ServiceBase = typeof(IDisposable))]
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Handles(ServiceBase = typeof(IDisposable))]
        public class ServiceBaseFromClass : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class WithoutServiceBase : IHandler<string, int>
        {
            public Task<int> RunAsync(string request, ServerCallContext context)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void GetFullClassName()
        {
            var expected = "global::" + typeof(TypeExtensionTests).FullName + "." + nameof(WithoutServiceBase);
            
            var result = typeof(WithoutServiceBase).GetFullClassName();

            result.Should().Be(expected);
        }

        [TestMethod]
        public void GetGeneratedDispatcherName()
        {
            var result = typeof(TestNameServiceBase).GetGeneratedDispatcherName();

            result.Should().Be("TestNameServiceDispatcher");
        }

        public class TestNameServiceBase { }
    }
}