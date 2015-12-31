// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Modles an authorization code.
    /// </summary>
    public class AuthorizationCode : ITokenMetadata
    {
        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this code is an OpenID Connect code.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is open identifier; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpenId { get; set; }
        
        /// <summary>
        /// Gets or sets the requested scopes.
        /// </summary>
        /// <value>
        /// The requested scopes.
        /// </value>
        public IEnumerable<Scope> RequestedScopes { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        /// <value>
        /// The nonce.
        /// </value>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether consent was shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent was shown; otherwise, <c>false</c>.
        /// </value>
        public bool WasConsentShown { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationCode"/> class.
        /// </summary>
        public AuthorizationCode()
        {
            CreationTime = DateTimeOffsetHelper.UtcNow;
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <value>
        /// The subject identifier.
        /// </value>
        public string SubjectId
        {
            get { return Subject.GetSubjectId(); }
        }

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId
        {
            get { return Client.ClientId; }
        }

        /// <summary>
        /// Gets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> Scopes
        {
            get { return RequestedScopes.Select(x => x.Name); }
        }
    }
}