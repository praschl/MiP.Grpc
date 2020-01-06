using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace MiP.Grpc.CodeGen
{
    internal static class AttributeCodeGenerator
    {
        public static string GenerateAttributes(IReadOnlyCollection<AuthorizeAttribute> authorizeAttributes)
        {
            var lines = authorizeAttributes.Select(a => Code.AuthorizeAttributeCode
                .Replace(Tag.AttributeProperties, GenerateAttributeProperties(a), StringComparison.Ordinal));

            return string.Concat(lines);
        }

        private static string GenerateAttributeProperties(AuthorizeAttribute attribute)
        {
            var properties = new List<string>(3);

            if (attribute.Roles != null)
                properties.Add(Code.RolesPropertyCode.Replace(Tag.Roles, attribute.Roles, StringComparison.Ordinal));

            if (attribute.Policy != null)
                properties.Add(Code.PolicyPropertyCode.Replace(Tag.Policy, attribute.Policy, StringComparison.Ordinal));

            if (attribute.AuthenticationSchemes != null)
                properties.Add(Code.AuthenticationSchemesPropertyCode.Replace(Tag.AuthenticationSchemes, attribute.AuthenticationSchemes, StringComparison.Ordinal));

            return string.Join(", ", properties);
        }
    }
}
