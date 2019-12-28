using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace MiP.Grpc
{
    internal class MapGrpcServiceConfiguration : IMapGrpcServiceConfigurationBuilder
    {
        private readonly List<AuthorizeAttribute> _authorizeAttributes = new List<AuthorizeAttribute>();

        public IReadOnlyCollection<AuthorizeAttribute> GlobalAuthorizeAttributes
        {
            get => _authorizeAttributes;
        }

        public void AddGlobalAuthorizeAttribute(AuthorizeAttribute authorizeAttribute)
        {
            _authorizeAttributes.Add(authorizeAttribute);
        }

        public void AddGlobalAuthorizeAttribute(string policy, string roles, string authenticationSchemes)
        {
            _authorizeAttributes.Add(new AuthorizeAttribute(policy) { Roles = roles, AuthenticationSchemes = authenticationSchemes });
        }
    }
}
