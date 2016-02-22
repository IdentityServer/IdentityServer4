// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models a token.
    /// </summary>
    public class Token : ITokenMetadata
    {
        /// <summary>
        /// Gets or sets the audience.
        /// </summary>
        /// <value>
        /// The audience.
        /// </value>
        public string Audience { get; set; }
        
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; set; }
        
        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        public DateTimeOffset CreationTime { get; set; }
        
        /// <summary>
        /// Gets or sets the lifetime.
        /// </summary>
        /// <value>
        /// The lifetime.
        /// </value>
        public int Lifetime { get; set; }
        
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public List<Claim> Claims { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public int Version { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        public Token()
        {
            Version = 3;
            Type = OidcConstants.TokenTypes.AccessToken;
            CreationTime = DateTimeOffsetHelper.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        public Token(string tokenType) : this()
        {
            Type = tokenType;
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <value>
        /// The subject identifier.
        /// </value>
        public string SubjectId
        {
            get
            {
                return Claims.Where(x => x.Type == JwtClaimTypes.Subject).Select(x => x.Value).SingleOrDefault();
            }
        }

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId
        {
            get
            {
                return Client.ClientId;
            }
        }

        /// <summary>
        /// Gets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> Scopes
        {
            get { return Claims.Where(x => x.Type == JwtClaimTypes.Scope).Select(x => x.Value); }
        }
    }
}