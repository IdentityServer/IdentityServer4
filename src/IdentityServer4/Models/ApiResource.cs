using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models a web API resource.
    /// </summary>
    public class ApiResource
    {
        /// <summary>
        /// Specifies if API is enabled (defaults to true)
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the api secrets.
        /// </summary>
        public ICollection<Secret> ApiSecrets { get; set; } = new HashSet<Secret>();

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        public ICollection<Scope> Scopes { get; set; } = new HashSet<Scope>();

        /// <summary>
        /// If enabled, all claims for the user will be included in the token. Defaults to false.
        /// </summary>
        public bool IncludeAllClaimsForUser { get; set; } = false;
        
        /// <summary>
        /// List of user claims that should be included in the access token.
        /// </summary>
        public ICollection<ScopeClaim> UserClaims { get; set; } = new HashSet<ScopeClaim>();

        /// <summary>
        /// Rule for determining which claims should be included in the token (this is implementation specific)
        /// </summary>
        public string ClaimsRule { get; set; }

        internal ApiResource CloneWithScopes(IEnumerable<Scope> scopes)
        {
            return new ApiResource()
            {
                Enabled = Enabled,
                Name = Name,
                ApiSecrets = ApiSecrets,
                Scopes = new HashSet<Scope>(scopes.ToArray()),
                UserClaims = UserClaims,
                IncludeAllClaimsForUser = IncludeAllClaimsForUser,
                ClaimsRule = ClaimsRule
            };
        }

        internal ApiResource CloneWithEnabledScopes()
        {
            return CloneWithScopes(Scopes.Where(x => x.Enabled));
        }
    }
}
