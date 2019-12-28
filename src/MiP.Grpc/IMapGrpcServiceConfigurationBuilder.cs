using Microsoft.AspNetCore.Authorization;

namespace MiP.Grpc
{
    /// <summary>
    /// Used for building configuration for compiling a grpc service map.
    /// </summary>
    public interface IMapGrpcServiceConfigurationBuilder
    {
        /// <summary>
        /// Adds the <see cref="AuthorizeAttribute"/> to the generated class.
        /// </summary>
        /// <param name="authorizeAttribute">Attribute to add.</param>
        void AddGlobalAuthorizeAttribute(AuthorizeAttribute authorizeAttribute);

        /// <summary>
        /// Creates an <see cref="AuthorizeAttribute"/> from the parameters and adds it to the generated class.
        /// </summary>
        /// <param name="policy"><see cref="AuthorizeAttribute.Policy"/></param>
        /// <param name="roles"><see cref="AuthorizeAttribute.Roles"/></param>
        /// <param name="authenticationSchemes"><see cref="AuthorizeAttribute.AuthenticationSchemes"/></param>
        void AddGlobalAuthorizeAttribute(string policy, string roles, string authenticationSchemes);
    }
}
