// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services.Default
{
    /// <summary>
    /// Default token service
    /// </summary>
    public class DefaultTokenService : ITokenService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The identity server context
        /// </summary>
        protected readonly IdentityServerContext _context;

        /// <summary>
        /// The claims provider
        /// </summary>
        protected readonly IClaimsProvider _claimsProvider;

        /// <summary>
        /// The token handles
        /// </summary>
        protected readonly ITokenHandleStore _tokenHandles;

        /// <summary>
        /// The signing service
        /// </summary>
        protected readonly ITokenSigningService _signingService;

        /// <summary>
        /// The events service
        /// </summary>
        protected readonly IEventService _events;
        
        // todo
        //protected readonly OwinEnvironmentService _owinEnvironmentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenService" /> class. This overloaded constructor is deprecated and will be removed in 3.0.0.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="claimsProvider">The claims provider.</param>
        /// <param name="tokenHandles">The token handles.</param>
        /// <param name="signingService">The signing service.</param>
        /// <param name="events">The events service.</param>
        public DefaultTokenService(IdentityServerContext context, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles, ITokenSigningService signingService, IEventService events, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DefaultTokenService>();
            _context = context;
            _claimsProvider = claimsProvider;
            _tokenHandles = tokenHandles;
            _signingService = signingService;
            _events = events;
        }

        // todo
        ///// <summary>
        ///// Initializes a new instance of the <see cref="DefaultTokenService" /> class.
        ///// </summary>
        ///// <param name="options">The options.</param>
        ///// <param name="claimsProvider">The claims provider.</param>
        ///// <param name="tokenHandles">The token handles.</param>
        ///// <param name="signingService">The signing service.</param>
        ///// <param name="events">The OWIN environment service.</param>
        ///// <param name="owinEnvironmentService">The events service.</param>
        //public DefaultTokenService(IdentityServerOptions options, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles, ITokenSigningService signingService, IEventService events, OwinEnvironmentService owinEnvironmentService)
        //{
        //    _options = options;
        //    _claimsProvider = claimsProvider;
        //    _tokenHandles = tokenHandles;
        //    _signingService = signingService;
        //    _events = events;
        //    _owinEnvironmentService = owinEnvironmentService;
        //}

        /// <summary>
        /// Creates an identity token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An identity token
        /// </returns>
        public virtual async Task<Token> CreateIdentityTokenAsync(TokenCreationRequest request)
        {
            _logger.LogVerbose("Creating identity token");
            request.Validate();

            // host provided claims
            var claims = new List<Claim>();

            // if nonce was sent, must be mirrored in id token
            if (request.Nonce.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.Nonce, request.Nonce));
            }

            // add iat claim
            claims.Add(new Claim(JwtClaimTypes.IssuedAt, DateTimeOffsetHelper.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer));

            // add at_hash claim
            if (request.AccessTokenToHash.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.AccessTokenHash, HashAdditionalData(request.AccessTokenToHash)));
            }

            // add c_hash claim
            if (request.AuthorizationCodeToHash.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.AuthorizationCodeHash, HashAdditionalData(request.AuthorizationCodeToHash)));
            }

            // add sid if present
            if (request.ValidatedRequest.SessionId.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, request.ValidatedRequest.SessionId));
            }

            claims.AddRange(await _claimsProvider.GetIdentityTokenClaimsAsync(
                request.Subject,
                request.Client,
                request.Scopes,
                request.IncludeAllIdentityClaims,
                request.ValidatedRequest));

            var issuer = _context.GetIssuerUri();

            var token = new Token(OidcConstants.TokenTypes.IdentityToken)
            {
                Audience = request.Client.ClientId,
                Issuer = issuer,
                Lifetime = request.Client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                Client = request.Client
            };

            return token;
        }

        /// <summary>
        /// Creates an access token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An access token
        /// </returns>
        public virtual async Task<Token> CreateAccessTokenAsync(TokenCreationRequest request)
        {
            _logger.LogVerbose("Creating access token");
            request.Validate();

            var claims = new List<Claim>();
            claims.AddRange(await _claimsProvider.GetAccessTokenClaimsAsync(
                request.Subject,
                request.Client,
                request.Scopes,
                request.ValidatedRequest));

            if (request.Client.IncludeJwtId)
            {
                claims.Add(new Claim(JwtClaimTypes.JwtId, CryptoRandom.CreateUniqueId()));
            }

            var issuer = _context.GetIssuerUri();
            var token = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                Audience = string.Format(Constants.AccessTokenAudience, issuer.EnsureTrailingSlash()),
                Issuer = issuer,
                Lifetime = request.Client.AccessTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                Client = request.Client
            };

            return token;
        }

        /// <summary>
        /// Creates a serialized and protected security token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// A security token in serialized form
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Invalid token type.</exception>
        public virtual async Task<string> CreateSecurityTokenAsync(Token token)
        {
            string tokenResult;

            if (token.Type == OidcConstants.TokenTypes.AccessToken)
            {
                if (token.Client.AccessTokenType == AccessTokenType.Jwt)
                {
                    _logger.LogVerbose("Creating JWT access token");

                    tokenResult = await _signingService.SignTokenAsync(token);
                }
                else
                {
                    _logger.LogVerbose("Creating reference access token");

                    var handle = CryptoRandom.CreateUniqueId();
                    await _tokenHandles.StoreAsync(handle, token);

                    tokenResult = handle;
                }
            }
            else if (token.Type == OidcConstants.TokenTypes.IdentityToken)
            {
                _logger.LogVerbose("Creating JWT identity token");

                tokenResult = await _signingService.SignTokenAsync(token);
            }
            else
            {
                throw new InvalidOperationException("Invalid token type.");
            }

            await _events.RaiseTokenIssuedEventAsync(token, tokenResult);
            return tokenResult;
        }

        /// <summary>
        /// Hashes an additional data (e.g. for c_hash or at_hash).
        /// </summary>
        /// <param name="tokenToHash">The token to hash.</param>
        /// <returns></returns>
        protected virtual string HashAdditionalData(string tokenToHash)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(tokenToHash));

                var leftPart = new byte[16];
                Array.Copy(hash, leftPart, 16);

                return Base64Url.Encode(leftPart);
            }
        }
    }
}