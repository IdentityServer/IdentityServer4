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
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    public class TokenResponseGenerator : ITokenResponseGenerator
    {
        private ILogger _logger;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IScopeStore _scopes;
       
        public TokenResponseGenerator(ITokenService tokenService, IRefreshTokenService refreshTokenService, IScopeStore scopes, ILoggerFactory loggerFactory)
        {
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _scopes = scopes;
            _logger = loggerFactory.CreateLogger<TokenResponseGenerator>();
        }

        public async Task<TokenResponse> ProcessAsync(ValidatedTokenRequest request)
        {
            _logger.LogVerbose("Creating token response");

            if (request.GrantType == OidcConstants.GrantTypes.AuthorizationCode)
            {
                return await ProcessAuthorizationCodeRequestAsync(request);
            }

            if (request.GrantType == OidcConstants.GrantTypes.RefreshToken)
            {
                return await ProcessRefreshTokenRequestAsync(request);
            }

            return await ProcessTokenRequestAsync(request);
        }

        private async Task<TokenResponse> ProcessAuthorizationCodeRequestAsync(ValidatedTokenRequest request)
        {
            _logger.LogVerbose("Processing authorization code request");

            //////////////////////////
            // access token
            /////////////////////////
            var accessToken = await CreateAccessTokenAsync(request);
            var response = new TokenResponse
            {
                AccessToken = accessToken.Item1,
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            //////////////////////////
            // refresh token
            /////////////////////////
            if (accessToken.Item2.IsPresent())
            {
                response.RefreshToken = accessToken.Item2;
            }

            //////////////////////////
            // id token
            /////////////////////////
            if (request.AuthorizationCode.IsOpenId)
            {
                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Client = request.AuthorizationCode.Client,
                    Scopes = request.AuthorizationCode.RequestedScopes,
                    Nonce = request.AuthorizationCode.Nonce,

                    ValidatedRequest = request
                };

                var idToken = await _tokenService.CreateIdentityTokenAsync(tokenRequest);
                var jwt = await _tokenService.CreateSecurityTokenAsync(idToken);
                response.IdentityToken = jwt;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessTokenRequestAsync(ValidatedTokenRequest request)
        {
            _logger.LogVerbose("Processing token request");

            var accessToken = await CreateAccessTokenAsync(request);
            var response = new TokenResponse
            {
                AccessToken = accessToken.Item1,
                AccessTokenLifetime = request.Client.AccessTokenLifetime
            };

            if (accessToken.Item2.IsPresent())
            {
                response.RefreshToken = accessToken.Item2;
            }

            return response;
        }

        private async Task<TokenResponse> ProcessRefreshTokenRequestAsync(ValidatedTokenRequest request)
        {
            _logger.LogVerbose("Processing refresh token request");

            var oldAccessToken = request.RefreshToken.AccessToken;
            string accessTokenString;
            
            if (request.Client.UpdateAccessTokenClaimsOnRefresh)
            {
                var subject = request.RefreshToken.GetOriginalSubject();

                var creationRequest = new TokenCreationRequest
                {
                    Client = request.Client,
                    Subject = subject,
                    ValidatedRequest = request,
                    Scopes = await _scopes.FindScopesAsync(oldAccessToken.Scopes)
                };

                var newAccessToken = await _tokenService.CreateAccessTokenAsync(creationRequest);
                accessTokenString = await _tokenService.CreateSecurityTokenAsync(newAccessToken);
            }
            else
            {
                oldAccessToken.CreationTime = DateTimeOffsetHelper.UtcNow;
                oldAccessToken.Lifetime = request.Client.AccessTokenLifetime;

                accessTokenString = await _tokenService.CreateSecurityTokenAsync(oldAccessToken);
            }

            var handle = await _refreshTokenService.UpdateRefreshTokenAsync(request.RefreshTokenHandle, request.RefreshToken, request.Client);

            return new TokenResponse
                {
                    AccessToken = accessTokenString,
                    AccessTokenLifetime = request.Client.AccessTokenLifetime,
                    RefreshToken = handle
                };
        }

        private async Task<Tuple<string, string>> CreateAccessTokenAsync(ValidatedTokenRequest request)
        {
            TokenCreationRequest tokenRequest;
            bool createRefreshToken;

            if (request.AuthorizationCode != null)
            {
                createRefreshToken = request.AuthorizationCode.RequestedScopes.Select(s => s.Name).Contains(Constants.StandardScopes.OfflineAccess);
                
                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Client = request.AuthorizationCode.Client,
                    Scopes = request.AuthorizationCode.RequestedScopes,
                    ValidatedRequest = request
                };
            }
            else
            {
                createRefreshToken = request.ValidatedScopes.ContainsOfflineAccessScope;

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.Subject,
                    Client = request.Client,
                    Scopes = request.ValidatedScopes.GrantedScopes,
                    ValidatedRequest = request
                };
            }

            Token accessToken = await _tokenService.CreateAccessTokenAsync(tokenRequest);

            string refreshToken = "";
            if (createRefreshToken)
            {
                refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(tokenRequest.Subject, accessToken, request.Client);
            }

            var securityToken = await _tokenService.CreateSecurityTokenAsync(accessToken);
            return Tuple.Create(securityToken, refreshToken);
        }
    }
}