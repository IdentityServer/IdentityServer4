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
using Microsoft.AspNetCore.Authentication;
using System.Threading;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// The default token response generator
    /// </summary>
    /// <seealso cref="IdentityServer4.ResponseHandling.ITokenResponseGenerator" />
    public class TokenResponseGenerator : ITokenResponseGenerator
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The token service
        /// </summary>
        protected readonly ITokenService TokenService;

        /// <summary>
        /// The refresh token service
        /// </summary>
        protected readonly IRefreshTokenService RefreshTokenService;

        /// <summary>
        /// The scope parser
        /// </summary>
        public IScopeParser ScopeParser { get; }

        /// <summary>
        /// The resource store
        /// </summary>
        protected readonly IResourceStore Resources;

        /// <summary>
        /// The clients store
        /// </summary>
        protected readonly IClientStore Clients;

        /// <summary>
        ///  The clock
        /// </summary>
        protected readonly ISystemClock Clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenResponseGenerator" /> class.
        /// </summary>
        /// <param name="clock">The clock.</param>
        /// <param name="tokenService">The token service.</param>
        /// <param name="refreshTokenService">The refresh token service.</param>
        /// <param name="scopeParser">The scope parser.</param>
        /// <param name="resources">The resources.</param>
        /// <param name="clients">The clients.</param>
        /// <param name="logger">The logger.</param>
        public TokenResponseGenerator(ISystemClock clock, ITokenService tokenService, IRefreshTokenService refreshTokenService, IScopeParser scopeParser, IResourceStore resources, IClientStore clients, ILogger<TokenResponseGenerator> logger)
        {
            Clock = clock;
            TokenService = tokenService;
            RefreshTokenService = refreshTokenService;
            ScopeParser = scopeParser;
            Resources = resources;
            Clients = clients;
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task<TokenResponse> ProcessAsync(TokenRequestValidationResult request, CancellationToken cancellationToken = default)
        {
            switch (request.ValidatedRequest.GrantType)
            {
                case OidcConstants.GrantTypes.ClientCredentials:
                    return await ProcessClientCredentialsRequestAsync(request, cancellationToken);
                case OidcConstants.GrantTypes.Password:
                    return await ProcessPasswordRequestAsync(request);
                case OidcConstants.GrantTypes.AuthorizationCode:
                    return await ProcessAuthorizationCodeRequestAsync(request, cancellationToken);
                case OidcConstants.GrantTypes.RefreshToken:
                    return await ProcessRefreshTokenRequestAsync(request, cancellationToken);
                case OidcConstants.GrantTypes.DeviceCode:
                    return await ProcessDeviceCodeRequestAsync(request, cancellationToken);
                default:
                    return await ProcessExtensionGrantRequestAsync(request, cancellationToken);
            }
        }

        /// <summary>
        /// Creates the response for an client credentials request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        protected virtual Task<TokenResponse> ProcessClientCredentialsRequestAsync(TokenRequestValidationResult request, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Creating response for client credentials request");

            return ProcessTokenRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Creates the response for a password request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        protected virtual Task<TokenResponse> ProcessPasswordRequestAsync(TokenRequestValidationResult request, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Creating response for password request");

            return ProcessTokenRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Creates the response for an authorization code request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Client does not exist anymore.</exception>
        protected virtual async Task<TokenResponse> ProcessAuthorizationCodeRequestAsync(TokenRequestValidationResult request, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Creating response for authorization code request");

            //////////////////////////
            // access token
            /////////////////////////
            var (accessToken, refreshToken) = await CreateAccessTokenAsync(request.ValidatedRequest, cancellationToken);
            var response = new TokenResponse
            {
                AccessToken = accessToken,
                AccessTokenLifetime = request.ValidatedRequest.AccessTokenLifetime,
                Custom = request.CustomResponse,
                Scope = request.ValidatedRequest.AuthorizationCode.RequestedScopes.ToSpaceSeparatedString()
            };

            //////////////////////////
            // refresh token
            /////////////////////////
            if (refreshToken.IsPresent())
            {
                response.RefreshToken = refreshToken;
            }

            //////////////////////////
            // id token
            /////////////////////////
            if (request.ValidatedRequest.AuthorizationCode.IsOpenId)
            {
                // load the client that belongs to the authorization code
                Client client = null;
                if (request.ValidatedRequest.AuthorizationCode.ClientId != null)
                {
                    client = await Clients.FindEnabledClientByIdAsync(request.ValidatedRequest.AuthorizationCode.ClientId, cancellationToken);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                var parsedScopesResult = ScopeParser.ParseScopeValues(request.ValidatedRequest.AuthorizationCode.RequestedScopes);
                var validatedResources = await Resources.CreateResourceValidationResult(parsedScopesResult, cancellationToken);

                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.ValidatedRequest.AuthorizationCode.Subject,
                    ValidatedResources = validatedResources,
                    Nonce = request.ValidatedRequest.AuthorizationCode.Nonce,
                    AccessTokenToHash = response.AccessToken,
                    StateHash = request.ValidatedRequest.AuthorizationCode.StateHash,
                    ValidatedRequest = request.ValidatedRequest
                };

                var idToken = await TokenService.CreateIdentityTokenAsync(tokenRequest, cancellationToken);
                var jwt = await TokenService.CreateSecurityTokenAsync(idToken, cancellationToken);
                response.IdentityToken = jwt;
            }

            return response;
        }

        /// <summary>
        /// Creates the response for a refresh token request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        protected virtual async Task<TokenResponse> ProcessRefreshTokenRequestAsync(TokenRequestValidationResult request, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Creating response for refresh token request");

            var oldAccessToken = request.ValidatedRequest.RefreshToken.AccessToken;
            string accessTokenString;

            if (request.ValidatedRequest.Client.UpdateAccessTokenClaimsOnRefresh)
            {
                var subject = request.ValidatedRequest.RefreshToken.Subject;

                // todo: do we want to just parse here and build up validated result
                // or do we want to fully re-run validation here.
                var parsedScopesResult = ScopeParser.ParseScopeValues(oldAccessToken.Scopes);
                var validatedResources = await Resources.CreateResourceValidationResult(parsedScopesResult, cancellationToken);

                var creationRequest = new TokenCreationRequest
                {
                    Subject = subject,
                    Description = request.ValidatedRequest.RefreshToken.Description,
                    ValidatedRequest = request.ValidatedRequest,
                    ValidatedResources = validatedResources
                };

                var newAccessToken = await TokenService.CreateAccessTokenAsync(creationRequest, cancellationToken);
                accessTokenString = await TokenService.CreateSecurityTokenAsync(newAccessToken, cancellationToken);
            }
            else
            {
                oldAccessToken.CreationTime = Clock.UtcNow.UtcDateTime;
                oldAccessToken.Lifetime = request.ValidatedRequest.AccessTokenLifetime;

                accessTokenString = await TokenService.CreateSecurityTokenAsync(oldAccessToken, cancellationToken);
            }

            var handle = await RefreshTokenService.UpdateRefreshTokenAsync(request.ValidatedRequest.RefreshTokenHandle, request.ValidatedRequest.RefreshToken, request.ValidatedRequest.Client, cancellationToken);

            return new TokenResponse
            {
                IdentityToken = await CreateIdTokenFromRefreshTokenRequestAsync(request.ValidatedRequest, accessTokenString, cancellationToken),
                AccessToken = accessTokenString,
                AccessTokenLifetime = request.ValidatedRequest.AccessTokenLifetime,
                RefreshToken = handle,
                Custom = request.CustomResponse,
                Scope = request.ValidatedRequest.RefreshToken.Scopes.ToSpaceSeparatedString()
            };
        }

        /// <summary>
        /// Processes the response for device code grant request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        protected virtual async Task<TokenResponse> ProcessDeviceCodeRequestAsync(TokenRequestValidationResult request, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Creating response for device code request");

            //////////////////////////
            // access token
            /////////////////////////
            var (accessToken, refreshToken) = await CreateAccessTokenAsync(request.ValidatedRequest, cancellationToken);
            var response = new TokenResponse
            {
                AccessToken = accessToken,
                AccessTokenLifetime = request.ValidatedRequest.AccessTokenLifetime,
                Custom = request.CustomResponse,
                Scope = request.ValidatedRequest.DeviceCode.AuthorizedScopes.ToSpaceSeparatedString()
            };

            //////////////////////////
            // refresh token
            /////////////////////////
            if (refreshToken.IsPresent())
            {
                response.RefreshToken = refreshToken;
            }

            //////////////////////////
            // id token
            /////////////////////////
            if (request.ValidatedRequest.DeviceCode.IsOpenId)
            {
                // load the client that belongs to the device code
                Client client = null;
                if (request.ValidatedRequest.DeviceCode.ClientId != null)
                {
                    client = await Clients.FindEnabledClientByIdAsync(request.ValidatedRequest.DeviceCode.ClientId, cancellationToken);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                var parsedScopesResult = ScopeParser.ParseScopeValues(request.ValidatedRequest.DeviceCode.AuthorizedScopes);
                var validatedResources = await Resources.CreateResourceValidationResult(parsedScopesResult, cancellationToken);
                
                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.ValidatedRequest.DeviceCode.Subject,
                    ValidatedResources = validatedResources,
                    AccessTokenToHash = response.AccessToken,
                    ValidatedRequest = request.ValidatedRequest
                };

                var idToken = await TokenService.CreateIdentityTokenAsync(tokenRequest, cancellationToken);
                var jwt = await TokenService.CreateSecurityTokenAsync(idToken, cancellationToken);
                response.IdentityToken = jwt;
            }

            return response;
        }

        /// <summary>
        /// Creates the response for an extension grant request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        protected virtual Task<TokenResponse> ProcessExtensionGrantRequestAsync(TokenRequestValidationResult request, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Creating response for extension grant request");

            return ProcessTokenRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Creates the response for a token request.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        protected virtual async Task<TokenResponse> ProcessTokenRequestAsync(TokenRequestValidationResult validationResult, CancellationToken cancellationToken = default)
        {
            (var accessToken, var refreshToken) = await CreateAccessTokenAsync(validationResult.ValidatedRequest, cancellationToken);
            var response = new TokenResponse
            {
                AccessToken = accessToken,
                AccessTokenLifetime = validationResult.ValidatedRequest.AccessTokenLifetime,
                Custom = validationResult.CustomResponse,
                Scope = validationResult.ValidatedRequest.ValidatedResources.RawScopeValues.ToSpaceSeparatedString()
            };

            if (refreshToken.IsPresent())
            {
                response.RefreshToken = refreshToken;
            }

            return response;
        }

        /// <summary>
        /// Creates the access/refresh token.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Client does not exist anymore.</exception>
        protected virtual async Task<(string accessToken, string refreshToken)> CreateAccessTokenAsync(ValidatedTokenRequest request, CancellationToken cancellationToken = default)
        {
            TokenCreationRequest tokenRequest;
            bool createRefreshToken;

            if (request.AuthorizationCode != null)
            {
                createRefreshToken = request.AuthorizationCode.RequestedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

                // load the client that belongs to the authorization code
                Client client = null;
                if (request.AuthorizationCode.ClientId != null)
                {
                    client = await Clients.FindEnabledClientByIdAsync(request.AuthorizationCode.ClientId, cancellationToken);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                var parsedScopesResult = ScopeParser.ParseScopeValues(request.AuthorizationCode.RequestedScopes);
                var validatedResources = await Resources.CreateResourceValidationResult(parsedScopesResult, cancellationToken);

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.AuthorizationCode.Subject,
                    Description = request.AuthorizationCode.Description,
                    ValidatedResources = validatedResources,
                    ValidatedRequest = request
                };
            }
            else if (request.DeviceCode != null)
            {
                createRefreshToken = request.DeviceCode.AuthorizedScopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

                Client client = null;
                if (request.DeviceCode.ClientId != null)
                {
                    client = await Clients.FindEnabledClientByIdAsync(request.DeviceCode.ClientId, cancellationToken);
                }
                if (client == null)
                {
                    throw new InvalidOperationException("Client does not exist anymore.");
                }

                var parsedScopesResult = ScopeParser.ParseScopeValues(request.DeviceCode.AuthorizedScopes);
                var validatedResources = await Resources.CreateResourceValidationResult(parsedScopesResult, cancellationToken);

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.DeviceCode.Subject,
                    Description = request.DeviceCode.Description,
                    ValidatedResources = validatedResources,
                    ValidatedRequest = request
                };
            }
            else
            {
                createRefreshToken = request.ValidatedResources.Resources.OfflineAccess;

                tokenRequest = new TokenCreationRequest
                {
                    Subject = request.Subject,
                    ValidatedResources = request.ValidatedResources,
                    ValidatedRequest = request
                };
            }

            var at = await TokenService.CreateAccessTokenAsync(tokenRequest, cancellationToken);
            var accessToken = await TokenService.CreateSecurityTokenAsync(at, cancellationToken);

            if (createRefreshToken)
            {
                var refreshToken = await RefreshTokenService.CreateRefreshTokenAsync(tokenRequest.Subject, at, request.Client, cancellationToken);
                return (accessToken, refreshToken);
            }

            return (accessToken, null);
        }

        /// <summary>
        /// Creates an id_token for a refresh token request if identity resources have been requested.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="newAccessToken">The new access token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        protected virtual async Task<string> CreateIdTokenFromRefreshTokenRequestAsync(ValidatedTokenRequest request, string newAccessToken, CancellationToken cancellationToken = default)
        {
            // todo: can we just check for "openid" scope?
            //var identityResources = await Resources.FindEnabledIdentityResourcesByScopeAsync(request.RefreshToken.Scopes);
            //if (identityResources.Any())
            
            if (request.RefreshToken.Scopes.Contains(OidcConstants.StandardScopes.OpenId))
            {
                var oldAccessToken = request.RefreshToken.AccessToken;

                var parsedScopesResult = ScopeParser.ParseScopeValues(oldAccessToken.Scopes);
                var validatedResources = await Resources.CreateResourceValidationResult(parsedScopesResult, cancellationToken);

                var tokenRequest = new TokenCreationRequest
                {
                    Subject = request.RefreshToken.Subject,
                    ValidatedResources = validatedResources,
                    ValidatedRequest = request,
                    AccessTokenToHash = newAccessToken
                };

                var idToken = await TokenService.CreateIdentityTokenAsync(tokenRequest, cancellationToken);
                return await TokenService.CreateSecurityTokenAsync(idToken, cancellationToken);
            }

            return null;
        }
    }
}