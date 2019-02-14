using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Hosting.LocalAccessTokenValidation
{
    /// <summary>
    /// Options for local access token authentication
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions" />
    public class LocalAccessTokenValidationOptions : AuthenticationSchemeOptions
    {
        public string ExpectedScope { get; set; }

        /// <summary>
        /// specifies whether the token should be saved in the authentication properties
        /// </summary>
        public bool SaveToken { get; set; } = true;

        /// <summary>
        /// Claim type for name
        /// </summary>
        public string NameClaimType { get; set; } = "name";

        /// <summary>
        /// Claim type for role
        /// </summary>
        public string RoleClaimType { get; set; } = "role";
    }
}