// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System;
using IdentityModel;
using System.Collections;
using System.Diagnostics;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models an OpenID Connect or OAuth2 client
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Client
    {
        // setting grant types should be atomic
        private ICollection<string> _allowedGrantTypes = new GrantTypeValidatingHashSet();

        private string DebuggerDisplay => ClientId ?? $"{{{typeof(Client)}}}";

        /// <summary>
        /// Specifies if client is enabled (defaults to <c>true</c>)
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Unique ID of the client
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the protocol type.
        /// </summary>
        /// <value>
        /// The protocol type.
        /// </value>
        public string ProtocolType { get; set; } = IdentityServerConstants.ProtocolTypes.OpenIdConnect;

        /// <summary>
        /// Client secrets - only relevant for flows that require a secret
        /// </summary>
        public ICollection<Secret> ClientSecrets { get; set; } = new HashSet<Secret>();

        /// <summary>
        /// If set to false, no client secret is needed to request tokens at the token endpoint (defaults to <c>true</c>)
        /// </summary>
        public bool RequireClientSecret { get; set; } = true;

        /// <summary>
        /// Client display name (used for logging and consent screen)
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Description of the client.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URI to further information about client (used on consent screen)
        /// </summary>
        public string ClientUri { get; set; }

        /// <summary>
        /// URI to client logo (used on consent screen)
        /// </summary>
        public string LogoUri { get; set; }

        /// <summary>
        /// Specifies whether a consent screen is required (defaults to <c>true</c>)
        /// </summary>
        public bool RequireConsent { get; set; } = true;

        /// <summary>
        /// Specifies whether user can choose to store consent decisions (defaults to <c>true</c>)
        /// </summary>
        public bool AllowRememberConsent { get; set; } = true;

        /// <summary>
        /// Specifies the allowed grant types (legal combinations of AuthorizationCode, Implicit, Hybrid, ResourceOwner, ClientCredentials).
        /// </summary>
        public ICollection<string> AllowedGrantTypes
        {
            get { return _allowedGrantTypes; }
            set
            {
                ValidateGrantTypes(value);
                _allowedGrantTypes = new GrantTypeValidatingHashSet(value);
            }
        }

        /// <summary>
        /// Specifies whether a proof key is required for authorization code based token requests (defaults to <c>false</c>).
        /// </summary>
        public bool RequirePkce { get; set; } = false;

        /// <summary>
        /// Specifies whether a proof key can be sent using plain method (not recommended and defaults to <c>false</c>.)
        /// </summary>
        public bool AllowPlainTextPkce { get; set; } = false;

        /// <summary>
        /// Controls whether access tokens are transmitted via the browser for this client (defaults to <c>false</c>).
        /// This can prevent accidental leakage of access tokens when multiple response types are allowed.
        /// </summary>
        /// <value>
        /// <c>true</c> if access tokens can be transmitted via the browser; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAccessTokensViaBrowser { get; set; } = false;

        /// <summary>
        /// Specifies allowed URIs to return tokens or authorization codes to
        /// </summary>
        public ICollection<string> RedirectUris { get; set; } = new HashSet<string>();

        /// <summary>
        /// Specifies allowed URIs to redirect to after logout
        /// </summary>
        public ICollection<string> PostLogoutRedirectUris { get; set; } = new HashSet<string>();

        /// <summary>
        /// Specifies logout URI at client for HTTP front-channel based logout.
        /// </summary>
        public string FrontChannelLogoutUri { get; set; }

        /// <summary>
        /// Specifies is the user's session id should be sent to the FrontChannelLogoutUri. Defaults to <c>true</c>.
        /// </summary>
        public bool FrontChannelLogoutSessionRequired { get; set; } = true;

        /// <summary>
        /// Specifies logout URI at client for HTTP back-channel based logout.
        /// </summary>
        public string BackChannelLogoutUri { get; set; }

        /// <summary>
        /// Specifies is the user's session id should be sent to the BackChannelLogoutUri. Defaults to <c>true</c>.
        /// </summary>
        public bool BackChannelLogoutSessionRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [allow offline access]. Defaults to <c>false</c>.
        /// </summary>
        public bool AllowOfflineAccess { get; set; } = false;

        /// <summary>
        /// Specifies the api scopes that the client is allowed to request. If empty, the client can't access any scope
        /// </summary>
        public ICollection<string> AllowedScopes { get; set; } = new HashSet<string>();

        /// <summary>
        /// When requesting both an id token and access token, should the user claims always be added to the id token instead of requring the client to use the userinfo endpoint.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool AlwaysIncludeUserClaimsInIdToken { get; set; } = false;

        /// <summary>
        /// Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
        /// </summary>
        public int IdentityTokenLifetime { get; set; } = 300;

        /// <summary>
        /// Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
        /// </summary>
        public int AccessTokenLifetime { get; set; } = 3600;

        /// <summary>
        /// Lifetime of authorization code in seconds (defaults to 300 seconds / 5 minutes)
        /// </summary>
        public int AuthorizationCodeLifetime { get; set; } = 300;

        /// <summary>
        /// Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds / 30 days
        /// </summary>
        public int AbsoluteRefreshTokenLifetime { get; set; } = 2592000;

        /// <summary>
        /// Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds / 15 days
        /// </summary>
        public int SlidingRefreshTokenLifetime { get; set; } = 1296000;

        /// <summary>
        /// Lifetime of a user consent in seconds. Defaults to null (no expiration)
        /// </summary>
        public int? ConsentLifetime { get; set; } = null;

        /// <summary>
        /// ReUse: the refresh token handle will stay the same when refreshing tokens
        /// OneTime: the refresh token handle will be updated when refreshing tokens
        /// </summary>
        public TokenUsage RefreshTokenUsage { get; set; } = TokenUsage.OneTimeOnly;

        /// <summary>
        /// Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request.
        /// Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if the token should be updated; otherwise, <c>false</c>.
        /// </value>
        public bool UpdateAccessTokenClaimsOnRefresh { get; set; } = false;

        /// <summary>
        /// Absolute: the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
        /// Sliding: when refreshing the token, the lifetime of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime). The lifetime will not exceed AbsoluteRefreshTokenLifetime.
        /// </summary>        
        public TokenExpiration RefreshTokenExpiration { get; set; } = TokenExpiration.Absolute;

        /// <summary>
        /// Specifies whether the access token is a reference token or a self contained JWT token (defaults to Jwt).
        /// </summary>
        public AccessTokenType AccessTokenType { get; set; } = AccessTokenType.Jwt;

        /// <summary>
        /// Gets or sets a value indicating whether the local login is allowed for this client. Defaults to <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if local logins are enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableLocalLogin { get; set; } = true;

        /// <summary>
        /// Specifies which external IdPs can be used with this client (if list is empty all IdPs are allowed). Defaults to empty.
        /// </summary>
        public ICollection<string> IdentityProviderRestrictions { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets a value indicating whether JWT access tokens should include an identifier. Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> to add an id; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeJwtId { get; set; } = false;

        /// <summary>
        /// Allows settings claims for the client (will be included in the access token).
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public ICollection<Claim> Claims { get; set; } = new HashSet<Claim>(new ClaimComparer());

        /// <summary>
        /// Gets or sets a value indicating whether client claims should be always included in the access tokens - or only for client credentials flow.
        /// Defaults to <c>false</c>
        /// </summary>
        /// <value>
        /// <c>true</c> if claims should always be sent; otherwise, <c>false</c>.
        /// </value>
        public bool AlwaysSendClientClaims { get; set; } = false;

        /// <summary>
        /// Gets or sets a value to prefix it on client claim types. Defaults to <c>client_</c>.
        /// </summary>
        /// <value>
        /// Any non empty string if claims should be prefixed with the value; otherwise, <c>null</c>.
        /// </value>
        public string ClientClaimsPrefix { get; set; } = "client_";

        /// <summary>
        /// Gets or sets a salt value used in pair-wise subjectId generation for users of this client.
        /// </summary>
        public string PairWiseSubjectSalt { get; set; }

        /// <summary>
        /// The maximum duration (in seconds) since the last time the user authenticated.
        /// </summary>
        public int? UserSsoLifetime { get; set; }

        /// <summary>
        /// Gets or sets the type of the device flow user code.
        /// </summary>
        /// <value>
        /// The type of the device flow user code.
        /// </value>
        public string UserCodeType { get; set; }

        /// <summary>
        /// Gets or sets the device code lifetime.
        /// </summary>
        /// <value>
        /// The device code lifetime.
        /// </value>
        public int DeviceCodeLifetime { get; set; } = 300;

        /// <summary>
        /// Gets or sets the allowed CORS origins for JavaScript clients.
        /// </summary>
        /// <value>
        /// The allowed CORS origins.
        /// </value>
        public ICollection<string> AllowedCorsOrigins { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the custom properties for the client.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Validates the grant types.
        /// </summary>
        /// <param name="grantTypes">The grant types.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Grant types list is empty
        /// or
        /// Grant types cannot contain spaces
        /// or
        /// Grant types list contains duplicate values
        /// </exception>
        public static void ValidateGrantTypes(IEnumerable<string> grantTypes)
        {
            if (grantTypes == null)
            {
                throw new ArgumentNullException(nameof(grantTypes));
            }

            // spaces are not allowed in grant types
            foreach (var type in grantTypes)
            {
                if (type.Contains(' '))
                {
                    throw new InvalidOperationException("Grant types cannot contain spaces");
                }
            }

            // single grant type, seems to be fine
            if (grantTypes.Count() == 1) return;

            // don't allow duplicate grant types
            if (grantTypes.Count() != grantTypes.Distinct().Count())
            {
                throw new InvalidOperationException("Grant types list contains duplicate values");
            }

            // would allow response_type downgrade attack from code to token
            DisallowGrantTypeCombination(GrantType.Implicit, GrantType.AuthorizationCode, grantTypes);
            DisallowGrantTypeCombination(GrantType.Implicit, GrantType.Hybrid, grantTypes);

            DisallowGrantTypeCombination(GrantType.AuthorizationCode, GrantType.Hybrid, grantTypes);
        }

        private static void DisallowGrantTypeCombination(string value1, string value2, IEnumerable<string> grantTypes)
        {
            if (grantTypes.Contains(value1, StringComparer.Ordinal) &&
                grantTypes.Contains(value2, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Grant types list cannot contain both {value1} and {value2}");
            }
        }

        internal class GrantTypeValidatingHashSet : ICollection<string>
        {
            private readonly ICollection<string> _inner;

            public GrantTypeValidatingHashSet()
            {
                _inner = new HashSet<string>();
            }

            public GrantTypeValidatingHashSet(IEnumerable<string> values)
            {
                _inner = new HashSet<string>(values);
            }

            private ICollection<string> Clone()
            {
                return new HashSet<string>(this);
            }

            private ICollection<string> CloneWith(params string[] values)
            {
                var clone = Clone();
                foreach (var item in values) clone.Add(item);
                return clone;
            }

            public int Count => _inner.Count;

            public bool IsReadOnly => _inner.IsReadOnly;

            public void Add(string item)
            {
                ValidateGrantTypes(CloneWith(item));
                _inner.Add(item);
            }

            public void Clear()
            {
                _inner.Clear();
            }

            public bool Contains(string item)
            {
                return _inner.Contains(item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                _inner.CopyTo(array, arrayIndex);
            }

            public IEnumerator<string> GetEnumerator()
            {
                return _inner.GetEnumerator();
            }

            public bool Remove(string item)
            {
                return _inner.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _inner.GetEnumerator();
            }
        }
    }
}
