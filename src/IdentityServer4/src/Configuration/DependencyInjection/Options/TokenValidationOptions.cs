using System.Collections.Generic;
using System.ComponentModel;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Configures the token validation.
    /// </summary>
    public class TokenValidationOptions
    {
        /// <summary>
        /// Gets or sets a boolean to control if the issuer will be validated during token validation.
        /// </summary>
        [DefaultValue(true)]
        public bool ValidateIssuer { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="IEnumerable{T}"/> that contains valid issuers that will be used to check against the token's issuer.
        /// </summary>
        public IEnumerable<string> ValidIssuers { get; set; }
    }
}