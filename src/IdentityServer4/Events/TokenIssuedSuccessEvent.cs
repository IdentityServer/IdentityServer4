﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Validation;
using System.Collections.Generic;
using static IdentityServer4.Constants;

namespace IdentityServer4.Events
{
    /// <summary>
    /// Event for successful token issuance
    /// </summary>
    /// <seealso cref="IdentityServer4.Events.Event" />
    public class TokenIssuedSuccessEvent : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenIssuedSuccessEvent"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        public TokenIssuedSuccessEvent(AuthorizeResponse response)
            : this()
        {
            ClientId = response.Request.ClientId;
            ClientName = response.Request.Client.ClientName;
            RedirectUri = response.RedirectUri;
            Endpoint = EndpointNames.Authorize;
            SubjectId = response.Request.Subject.GetSubjectId();
            Scopes = response.Scope;
            GrantType = response.Request.GrantType;

            var tokens = new List<Token>();
            if (response.IdentityToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.IdentityToken, response.IdentityToken));
            }
            if (response.Code != null)
            {
                tokens.Add(new Token(OidcConstants.ResponseTypes.Code, response.Code));
            }
            if (response.AccessToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.AccessToken, response.AccessToken));
            }
            Tokens = tokens;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenIssuedSuccessEvent"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="request">The request.</param>
        public TokenIssuedSuccessEvent(TokenResponse response, TokenRequestValidationResult request)
            : this()
        {
            ClientId = request.ValidatedRequest.Client.ClientId;
            ClientName = request.ValidatedRequest.Client.ClientName;
            Endpoint = EndpointNames.Token;
            SubjectId = request.ValidatedRequest.Subject?.GetSubjectId();
            GrantType = request.ValidatedRequest.GrantType;

            if (GrantType == OidcConstants.GrantTypes.RefreshToken)
            {
                Scopes = request.ValidatedRequest.RefreshToken.AccessToken.Scopes.ToSpaceSeparatedString();
            }
            else if (GrantType == OidcConstants.GrantTypes.AuthorizationCode)
            {
                Scopes = request.ValidatedRequest.AuthorizationCode.RequestedScopes.ToSpaceSeparatedString();
            }
            else
            {
                Scopes = request.ValidatedRequest.ValidatedScopes?.GrantedResources.ToScopeNames().ToSpaceSeparatedString();
            }

            var tokens = new List<Token>();
            if (response.IdentityToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.IdentityToken, response.IdentityToken));
            }
            if (response.RefreshToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.RefreshToken, response.RefreshToken));
            }
            if (response.AccessToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.AccessToken, response.AccessToken));
            }
            Tokens = tokens;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenIssuedSuccessEvent"/> class.
        /// </summary>
        protected TokenIssuedSuccessEvent()
            : base(EventCategories.Token,
                  "Token Issued Success",
                  EventTypes.Success,
                  EventIds.TokenIssuedSuccess)
        {
        }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the subject identifier.
        /// </summary>
        /// <value>
        /// The subject identifier.
        /// </value>
        public string SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public string Scopes { get; set; }

        /// <summary>
        /// Gets or sets the grant type.
        /// </summary>
        /// <value>
        /// The grant type.
        /// </value>
        public string GrantType { get; set; }

        /// <summary>
        /// Gets or sets the tokens.
        /// </summary>
        /// <value>
        /// The tokens.
        /// </value>
        public IEnumerable<Token> Tokens { get; set; }

        /// <summary>
        /// Data structure serializing issued tokens
        /// </summary>
        public class Token
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Token"/> class.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="value">The value.</param>
            public Token(string type, string value)
            {
                TokenType = type;
                TokenValue = Obfuscate(value);
            }

            /// <summary>
            /// Gets the type of the token.
            /// </summary>
            /// <value>
            /// The type of the token.
            /// </value>
            public string TokenType { get; }

            /// <summary>
            /// Gets the token value.
            /// </summary>
            /// <value>
            /// The token value.
            /// </value>
            public string TokenValue { get; }
        }
    }
}