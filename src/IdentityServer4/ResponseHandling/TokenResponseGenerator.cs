// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.ResponseHandling
{
    public class TokenResponseGenerator : ITokenResponseGenerator
    {
        private readonly ILogger _logger;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IResourceStore _resources;
        private readonly IClientStore _clients;

        public TokenResponseGenerator(ITokenService tokenService, IRefreshTokenService refreshTokenService, IResourceStore resources, IClientStore clients, ILoggerFactory loggerFactory)
        {
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _resources = resources;
            _clients = clients;
            _logger = loggerFactory.CreateLogger<TokenResponseGenerator>();
        }

        public async Task<TokenResponse> ProcessAsync(TokenRequestValidationResult validationResult)
        {
            _logger.LogTrace("Creating token response");

            if (validationResult.ValidatedRequest.GrantType == OidcConstants.GrantTypes.AuthorizationCode)
            {
                return await ProcessAuthorizationCodeRequestAsync(validationResult.ValidatedRequest);
            }

            if (validationResult.ValidatedRequest.GrantType == OidcConstants.GrantTypes.RefreshToken)
            {
                return await ProcessRefreshTokenRequestAsync(validationResult.ValidatedRequest);
            }

            return await ProcessTokenRequestAsync(validationResult);
        }

        private async Task<TokenResponse> ProcessAuthorizationCodeRequestAsync(ValidatedTokenRequest request)
        {
            _logger.LogTrace("Processing authorization code request");

            //////////////////////////
            // access token
            /////////////////////////
            var tokens = await CreateAccessTokenAsync(request);
            var response = new TokenResponse
            {
                AccessToken = tokens.AccessTokens,
                AccessTokenLifetime = request.AccessTokenLifetime
            };

            //////////////////////////
            // refresh token
            /////////////////////////
            if (tokens.RefreshToken.IsPresent())
            {
                response.RefreshToken = tokens.RefreshToken;
            }

            //////////////////////////
            // id token
            /////////////////////////
            if (request.AuthorizationCode.IsOpenId)
            {
                // load the client that belongs to the authorization code
                Client client = null;
                if (request.AuthorizationCode.ClientId != null)
                {
                    client = await _clients.FindEnabledClientByIdAsync(request.AuthorizationCode.ClientId);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                var resources = await _resources.FindEnabledResourcesByScopeAsync(request.AuthorizationCode.RequestedScopes);

                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Resources = resources,
                    Nonce = request.AuthorizationCode.Nonce,
                    AccessTokenToHash = response.AccessToken,
                    ValidatedRequest = request
                };

                var idToken = await _tokenService.CreateIdentityTokenAsync(tokenRequest);
                var jwt = await _tokenService.CreateSecurityTokenAsync(idToken);
                response.IdentityToken = jwt;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessTokenRequestAsync(TokenRequestValidationResult validationResult)
        {
            _logger.LogTrace("Processing token request");

            var tokens = await CreateAccessTokenAsync(validationResult.ValidatedRequest);
            var response = new TokenResponse
            {
                AccessToken = tokens.AccessTokens,
                AccessTokenLifetime = validationResult.ValidatedRequest.AccessTokenLifetime,
                Custom = validationResult.CustomResponse
            };

            if (tokens.RefreshToken.IsPresent())
            {
                response.RefreshToken = tokens.RefreshToken;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessRefreshTokenRequestAsync(ValidatedTokenRequest request)
        {
            _logger.LogTrace("Processing refresh token request");

            var oldAccessToken = request.RefreshToken.AccessToken;
            string accessTokenString;

            if (request.Client.UpdateAccessTokenClaimsOnRefresh)
            {
                var subject = request.RefreshToken.Subject;

                var creationRequest = new TokenCreationRequest
                {
                    Subject = subject,
                    ValidatedRequest = request,
                    Resources = await _resources.FindEnabledResourcesByScopeAsync(oldAccessToken.Scopes),
                };

                var newAccessToken = await _tokenService.CreateAccessTokenAsync(creationRequest);
                accessTokenString = await _tokenService.CreateSecurityTokenAsync(newAccessToken);
            }
            else
            {
                oldAccessToken.CreationTime = IdentityServerDateTime.UtcNow;
                oldAccessToken.Lifetime = request.AccessTokenLifetime;

                accessTokenString = await _tokenService.CreateSecurityTokenAsync(oldAccessToken);
            }

            var handle = await _refreshTokenService.UpdateRefreshTokenAsync(request.RefreshTokenHandle, request.RefreshToken, request.Client);

            return new TokenResponse
            {
                IdentityToken = await CreateIdTokenFromRefreshTokenRequestAsync(request, accessTokenString),
                AccessToken = accessTokenString,
                AccessTokenLifetime = request.AccessTokenLifetime,
                RefreshToken = handle
            };
        }

        private async Task<Tokens> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            TokenCreationRequest tokenRequest;
            bool createRefreshToken;
            var tokens = new Tokens();

            if (request.AuthorizationCode != null)
            {
                createRefreshToken = request.AuthorizationCode.RequestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

                // load the client that belongs to the authorization code
                Client client = null;
                if (request.AuthorizationCode.ClientId != null)
                {
                    client = await _clients.FindEnabledClientByIdAsync(request.AuthorizationCode.ClientId);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                var resources = await _resources.FindEnabledResourcesByScopeAsync(request.AuthorizationCode.RequestedScopes);

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Resources = resources,
                    ValidatedRequest = request
                };
            }
            else
            {
                createRefreshToken = request.ValidatedScopes.ContainsOfflineAccessScope;

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.Subject,
                    Resources = request.ValidatedScopes.GrantedResources,
                    ValidatedRequest = request
                };
            }

            Token accessToken = await _tokenService.CreateAccessTokenAsync(tokenRequest);
            tokens.AccessTokens = await _tokenService.CreateSecurityTokenAsync(accessToken);

            if (createRefreshToken)
            {
                tokens.RefreshToken = await _refreshTokenService.CreateRefreshTokenAsync(tokenRequest.Subject, accessToken, request.Client);
            }


            return tokens;
        }

        private async Task<string> CreateIdTokenFromRefreshTokenRequestAsync(ValidatedTokenRequest request, string newAccessToken)
        {
            var resources = await _resources.FindResourcesByScopeAsync(request.RefreshToken.Scopes);
            if (resources.IdentityResources.Any())
            {
                var oldAccessToken = request.RefreshToken.AccessToken;
                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.RefreshToken.Subject,
                    Resources = await _resources.FindEnabledResourcesByScopeAsync(oldAccessToken.Scopes),
                    ValidatedRequest = request,
                    AccessTokenToHash = newAccessToken
                };

                var idToken = await _tokenService.CreateIdentityTokenAsync(tokenRequest);
                return await _tokenService.CreateSecurityTokenAsync(idToken);
            }

            return null;
        }

        private class Tokens
        {
            public string AccessTokens { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}