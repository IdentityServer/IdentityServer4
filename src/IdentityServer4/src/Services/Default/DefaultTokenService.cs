// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default token service
    /// </summary>
    public class DefaultTokenService : ITokenService
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The HTTP context accessor
        /// </summary>
        protected readonly IHttpContextAccessor Context;

        /// <summary>
        /// The claims provider
        /// </summary>
        protected readonly IClaimsService ClaimsProvider;

        /// <summary>
        /// The reference token store
        /// </summary>
        protected readonly IReferenceTokenStore ReferenceTokenStore;

        /// <summary>
        /// The signing service
        /// </summary>
        protected readonly ITokenCreationService CreationService;

        /// <summary>
        /// The clock
        /// </summary>
        protected readonly ISystemClock Clock;

        /// <summary>
        /// The key material service
        /// </summary>
        protected readonly IKeyMaterialService KeyMaterialService;

        /// <summary>
        /// The IdentityServer options
        /// </summary>
        protected readonly IdentityServerOptions Options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenService" /> class.
        /// </summary>
        /// <param name="claimsProvider">The claims provider.</param>
        /// <param name="referenceTokenStore">The reference token store.</param>
        /// <param name="creationService">The signing service.</param>
        /// <param name="contextAccessor">The HTTP context accessor.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="keyMaterialService"></param>
        /// <param name="options">The IdentityServer options</param>
        /// <param name="logger">The logger.</param>
        public DefaultTokenService(
            IClaimsService claimsProvider,
            IReferenceTokenStore referenceTokenStore,
            ITokenCreationService creationService,
            IHttpContextAccessor contextAccessor,
            ISystemClock clock,
            IKeyMaterialService keyMaterialService,
            IdentityServerOptions options,
            ILogger<DefaultTokenService> logger)
        {
            Context = contextAccessor;
            ClaimsProvider = claimsProvider;
            ReferenceTokenStore = referenceTokenStore;
            CreationService = creationService;
            Clock = clock;
            KeyMaterialService = keyMaterialService;
            Options = options;
            Logger = logger;
        }

        /// <summary>
        /// Creates an identity token.
        /// </summary>
        /// <param name="request">The token creation request.</param>
        /// <returns>
        /// An identity token
        /// </returns>
        public virtual async Task<Token> CreateIdentityTokenAsync(TokenCreationRequest request)
        {
            Logger.LogTrace("Creating identity token");
            request.Validate();

            var credential = await KeyMaterialService.GetSigningCredentialsAsync();
            if (credential == null)
            {
                throw new InvalidOperationException("No signing credential is configured.");
            }

            var signingAlgorithm = credential.Algorithm;

            // host provided claims
            var claims = new List<Claim>();

            // if nonce was sent, must be mirrored in id token
            if (request.Nonce.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.Nonce, request.Nonce));
            }

            // add iat claim
            claims.Add(new Claim(JwtClaimTypes.IssuedAt, Clock.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            // add at_hash claim
            if (request.AccessTokenToHash.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.AccessTokenHash, CryptoHelper.CreateHashClaimValue(request.AccessTokenToHash, signingAlgorithm)));
            }

            // add c_hash claim
            if (request.AuthorizationCodeToHash.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.AuthorizationCodeHash, CryptoHelper.CreateHashClaimValue(request.AuthorizationCodeToHash, signingAlgorithm)));
            }

            // add s_hash claim
            if (request.StateHash.IsPresent())
            {
                // todo: need constant
                claims.Add(new Claim(JwtClaimTypes.StateHash, request.StateHash));
            }

            // add sid if present
            if (request.ValidatedRequest.SessionId.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, request.ValidatedRequest.SessionId));
            }

            claims.AddRange(await ClaimsProvider.GetIdentityTokenClaimsAsync(
                request.Subject,
                request.Resources,
                request.IncludeAllIdentityClaims,
                request.ValidatedRequest));

            var issuer = Context.HttpContext.GetIdentityServerIssuerUri();

            var token = new Token(OidcConstants.TokenTypes.IdentityToken)
            {
                CreationTime = Clock.UtcNow.UtcDateTime,
                Audiences = { request.ValidatedRequest.Client.ClientId },
                Issuer = issuer,
                Lifetime = request.ValidatedRequest.Client.IdentityTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                ClientId = request.ValidatedRequest.Client.ClientId,
                AccessTokenType = request.ValidatedRequest.AccessTokenType,
                AllowedSigningAlgorithms = request.ValidatedRequest.Client.AllowedIdentityTokenSigningAlgorithms
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
            Logger.LogTrace("Creating access token");
            request.Validate();

            var claims = new List<Claim>();
            claims.AddRange(await ClaimsProvider.GetAccessTokenClaimsAsync(
                request.Subject,
                request.Resources,
                request.ValidatedRequest));

            if (request.ValidatedRequest.Client.IncludeJwtId)
            {
                claims.Add(new Claim(JwtClaimTypes.JwtId, CryptoRandom.CreateUniqueId(16)));
            }

            var issuer = Context.HttpContext.GetIdentityServerIssuerUri();
            var token = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                CreationTime = Clock.UtcNow.UtcDateTime,
                Issuer = issuer,
                Lifetime = request.ValidatedRequest.AccessTokenLifetime,
                Claims = claims.Distinct(new ClaimComparer()).ToList(),
                ClientId = request.ValidatedRequest.Client.ClientId,
                AccessTokenType = request.ValidatedRequest.AccessTokenType
            };

            if (Options.EmitLegacyResourceAudienceClaim)
            {
                token.Audiences.Add(string.Format(IdentityServerConstants.AccessTokenAudience, issuer.EnsureTrailingSlash()));
            }

            //ICollection<string> allowedAlgorithms = new List<string>();
            //if (request.Resources.ApiResources.Any())
            //{
            //    allowedAlgorithms = request.Resources.ApiResources.First().AllowedSigningAlgorithms;
            //}

            foreach (var api in request.Resources.ApiResources)
            {
                if (api.Name.IsPresent())
                {
                    token.Audiences.Add(api.Name);
                }

                //allowedAlgorithms.Intersect(api.AllowedSigningAlgorithms);
            }

            // only one API resource request, forward the allowed signing algorithms (if any)
            if (request.Resources.ApiResources.Count == 1)
            {
                token.AllowedSigningAlgorithms = request.Resources.ApiResources.First().AllowedSigningAlgorithms;
            }
            else
            {
                var allAlgorithms = request.Resources.ApiResources.Where(r => r.AllowedSigningAlgorithms.Any()).Select(r => r.AllowedSigningAlgorithms);

                // resources need to agree on allowed signing algorithms
                if (allAlgorithms.Any())
                {
                    var allowedAlgorithms = IntersectLists(allAlgorithms);

                    if (allowedAlgorithms.Any())
                    {
                        token.AllowedSigningAlgorithms = allowedAlgorithms.ToHashSet();
                    }
                    else
                    {
                        throw new InvalidOperationException("Signing algorithms requirements for requested resources are not compatible.");
                    }
                }
            }

            return token;
        }

        private static IEnumerable<T> IntersectLists<T>(IEnumerable<IEnumerable<T>> lists)
        {
            return lists.Aggregate((l1, l2) => l1.Intersect(l2));
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
                if (token.AccessTokenType == AccessTokenType.Jwt)
                {
                    Logger.LogTrace("Creating JWT access token");

                    tokenResult = await CreationService.CreateTokenAsync(token);
                }
                else
                {
                    Logger.LogTrace("Creating reference access token");

                    var handle = await ReferenceTokenStore.StoreReferenceTokenAsync(token);

                    tokenResult = handle;
                }
            }
            else if (token.Type == OidcConstants.TokenTypes.IdentityToken)
            {
                Logger.LogTrace("Creating JWT identity token");

                tokenResult = await CreationService.CreateTokenAsync(token);
            }
            else
            {
                throw new InvalidOperationException("Invalid token type.");
            }

            return tokenResult;
        }
    }
}