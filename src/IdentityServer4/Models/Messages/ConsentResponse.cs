// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models the user's response to the consent screen.
    /// </summary>
    public class ConsentResponse
    {
        public static ConsentResponse Denied = new ConsentResponse();

        /// <summary>
        /// Gets if consent was granted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent was granted; otherwise, <c>false</c>.
        /// </value>
        public bool Granted
        {
            get
            {
                return ScopesConsented != null && ScopesConsented.Any();
            }
        }

        /// <summary>
        /// Gets or sets the scopes consented to.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> ScopesConsented { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wishes the consent to be remembered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent is to be remembered; otherwise, <c>false</c>.
        /// </value>
        public bool RememberConsent { get; set; }

        internal static string CreateId(string clientId, string subject, string nonce, IEnumerable<string> scopesRequested)
        {
            var normalizedScopes = scopesRequested.OrderBy(x => x).Distinct().Aggregate((x, y) => x + "," + y);
            var value = String.Format("{0}:{1}:{2}:{3}",
                clientId,
                subject,
                nonce,
                normalizedScopes);
            return value.Sha256();
        }
        internal static string CreateId(NameValueCollection parameters, string subject)
        {
            return CreateId(parameters[IdentityModel.OidcConstants.AuthorizeRequest.ClientId],
                subject,
                parameters[IdentityModel.OidcConstants.AuthorizeRequest.Nonce],
                parameters[IdentityModel.OidcConstants.AuthorizeRequest.Scope].ParseScopesString()
            );
        }
    }
}
