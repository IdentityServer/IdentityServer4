// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    class AuthorizeResponseGenerator : IAuthorizeResponseGenerator
    {
        private readonly ILogger<AuthorizeResponseGenerator> _logger;
        private readonly ITokenService _tokenService;
        private readonly IAuthorizationCodeStore _authorizationCodes;
        private readonly IEventService _events;

        public AuthorizeResponseGenerator(ILogger<AuthorizeResponseGenerator> logger, ITokenService tokenService, IAuthorizationCodeStore authorizationCodes, IEventService events)
        {
            _logger = logger;
            _tokenService = tokenService;
            _authorizationCodes = authorizationCodes;
            _events = events;
        }

        public async Task<AuthorizeResponse> CreateResponseAsync(ValidatedAuthorizeRequest request)
        {
            if (request.Flow == Flows.AuthorizationCode)
            {
                return await CreateCodeFlowResponseAsync(request);
            }
            if (request.Flow == Flows.Implicit)
            {
                return await CreateImplicitFlowResponseAsync(request);
            }
            if (request.Flow == Flows.Hybrid)
            {
                return await CreateHybridFlowResponseAsync(request);
            }

            _logger.LogError("Unsupported flow: " + request.Flow.ToString());
            throw new InvalidOperationException("invalid flow: " + request.Flow.ToString());
        }

        private async Task<AuthorizeResponse> CreateHybridFlowResponseAsync(ValidatedAuthorizeRequest request)
        {
            _logger.LogInformation("Creating Hybrid Flow response.");

            var code = await CreateCodeAsync(request);
            var response = await CreateImplicitFlowResponseAsync(request, code);
            response.Code = code;

            return response;
        }

        public async Task<AuthorizeResponse> CreateCodeFlowResponseAsync(ValidatedAuthorizeRequest request)
        {
            _logger.LogInformation("Creating Authorization Code Flow response.");

            var code = await CreateCodeAsync(request);

            var response = new AuthorizeResponse
            {
                Request = request,
                RedirectUri = request.RedirectUri,
                Code = code,
                State = request.State
            };

            if (request.IsOpenIdRequest)
            {
                response.SessionState = GenerateSessionStateValue(request);
            }

            return response;
        }

        private async Task<string> CreateCodeAsync(ValidatedAuthorizeRequest request)
        {
            var code = new AuthorizationCode
            {
                Client = request.Client,
                Subject = request.Subject,
                SessionId = request.SessionId,

                IsOpenId = request.IsOpenIdRequest,
                RequestedScopes = request.ValidatedScopes.GrantedScopes,
                RedirectUri = request.RedirectUri,
                Nonce = request.Nonce,

                WasConsentShown = request.WasConsentShown,
            };

            // store id token and access token and return authorization code
            var id = CryptoRandom.CreateUniqueId();
            await _authorizationCodes.StoreAsync(id, code);

            await RaiseCodeIssuedEventAsync(id, code);

            return id;
        }

        public async Task<AuthorizeResponse> CreateImplicitFlowResponseAsync(ValidatedAuthorizeRequest request, string authorizationCode = null)
        {
            _logger.LogInformation("Creating Implicit Flow response.");

            string accessTokenValue = null;
            int accessTokenLifetime = 0;

            var responseTypes = request.ResponseType.FromSpaceSeparatedString();

            if (responseTypes.Contains(OidcConstants.ResponseTypes.Token))
            {
                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.Subject,
                    Client = request.Client,
                    Scopes = request.ValidatedScopes.GrantedScopes,

                    ValidatedRequest = request
                };

                var accessToken = await _tokenService.CreateAccessTokenAsync(tokenRequest);
                accessTokenLifetime = accessToken.Lifetime;

                accessTokenValue = await _tokenService.CreateSecurityTokenAsync(accessToken);
            }

            string jwt = null;
            if (responseTypes.Contains(OidcConstants.ResponseTypes.IdToken))
            {
                var tokenRequest = new TokenCreationRequest
                {
                    ValidatedRequest = request,
                    Subject = request.Subject,
                    Client = request.Client,
                    Scopes = request.ValidatedScopes.GrantedScopes,

                    Nonce = request.Raw.Get(OidcConstants.AuthorizeRequest.Nonce),
                    IncludeAllIdentityClaims = !request.AccessTokenRequested,
                    AccessTokenToHash = accessTokenValue,
                    AuthorizationCodeToHash = authorizationCode
                };

                var idToken = await _tokenService.CreateIdentityTokenAsync(tokenRequest);
                jwt = await _tokenService.CreateSecurityTokenAsync(idToken);
            }

            var response = new AuthorizeResponse
            {
                Request = request,
                RedirectUri = request.RedirectUri,
                AccessToken = accessTokenValue,
                AccessTokenLifetime = accessTokenLifetime,
                IdentityToken = jwt,
                State = request.State,
                Scope = request.ValidatedScopes.GrantedScopes.ToSpaceSeparatedString(),
            };

            if (request.IsOpenIdRequest)
            {
                response.SessionState = GenerateSessionStateValue(request);
            }

            return response;
        }

        private string GenerateSessionStateValue(ValidatedAuthorizeRequest request)
        {
            var sessionId = request.SessionId;
            if (sessionId.IsMissing()) return null;

            var salt = CryptoRandom.CreateUniqueId();
            var clientId = request.ClientId;

            var uri = new Uri(request.RedirectUri);
            var origin = uri.Scheme + "://" + uri.Host;
            if (!uri.IsDefaultPort)
            {
                origin += ":" + uri.Port;
            }

            var bytes = Encoding.UTF8.GetBytes(clientId + origin + sessionId + salt);
            byte[] hash;

            using (var sha = SHA256.Create())
            {
                hash = sha.ComputeHash(bytes);
            }

            return Base64Url.Encode(hash) + "." + salt;
        }

        private async Task RaiseCodeIssuedEventAsync(string id, AuthorizationCode code)
        {
            await _events.RaiseAuthorizationCodeIssuedEventAsync(id, code);
        }
    }
}