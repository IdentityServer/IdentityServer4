using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models a user identity resource.
    /// </summary>
    public class IdentityResource
    {
        /// <summary>
        /// Indicates if scope is enabled and can be requested. Defaults to true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Name of the identity resource. This is the value a client will use to request the scope.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name. This value will be used e.g. on the consent screen.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Description. This value will be used e.g. on the consent screen.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
        /// </summary>
        public bool Emphasize { get; set; } = false;

        /// <summary>
        /// Specifies whether this scope is shown in the discovery document. Defaults to true.
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; } = true;
        
        /// <summary>
        /// List of user claims that should be included in the identity token.
        /// </summary>
        public ICollection<ScopeClaim> UserClaims { get; set; } = new HashSet<ScopeClaim>();

        /// <summary>
        /// If enabled, all claims for the user will be included in the token. Defaults to false.
        /// </summary>
        public bool IncludeAllClaimsForUser { get; set; } = false;

        /// <summary>
        /// Rule for determining which claims should be included in the token (this is implementation specific)
        /// </summary>
        public string ClaimsRule { get; set; }
    }
}
