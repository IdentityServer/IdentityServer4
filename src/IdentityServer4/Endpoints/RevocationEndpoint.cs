// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Stores;
using IdentityServer4.Events;

namespace IdentityServer4.Endpoints
{
    /// <summary>
    /// The revocation endpoint
    /// </summary>
    /// <seealso cref="IdentityServer4.Hosting.IEndpoint" />
    public class RevocationEndpoint : IEndpoint
    {
        private readonly ILogger _logger;
        private readonly ClientSecretValidator _clientValidator;
        private readonly ITokenRevocationRequestValidator _requestValidator;
        private readonly IReferenceTokenStore _referenceTokenStore;
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="RevocationEndpoint"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="clientValidator">The client validator.</param>
        /// <param name="requestValidator">The request validator.</param>
        /// <param name="referenceTokenStore">The reference token store.</param>
        /// <param name="refreshTokenStore">The refresh token store.</param>
        /// <param name="events">The events.</param>
        public RevocationEndpoint(ILogger<RevocationEndpoint> logger,
            ClientSecretValidator clientValidator,
            ITokenRevocationRequestValidator requestValidator,
            IReferenceTokenStore referenceTokenStore,
            IRefreshTokenStore refreshTokenStore,
            IEventService events)
        {
            _logger = logger;
            _clientValidator = clientValidator;
            _requestValidator = requestValidator;
            _referenceTokenStore = referenceTokenStore;
            _refreshTokenStore = refreshTokenStore;
            _events = events;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing revocation request.");

            if (context.Request.Method != "POST")
            {
                _logger.LogWarning("Invalid HTTP method");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (!context.Request.HasFormContentType)
            {
                _logger.LogWarning("Invalid media type");
                return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
            }

            var response = await ProcessRevocationRequestAsync(context);

            return response;
        }

        private async Task<IEndpointResult> ProcessRevocationRequestAsync(HttpContext context)
        {
            _logger.LogDebug("Start revocation request.");

            // validate client
            var clientResult = await _clientValidator.ValidateAsync(context);

            var client = clientResult.Client;
            if (client == null)
            {
                return new RevocationErrorResult(OidcConstants.TokenErrors.InvalidClient);
            }

            _logger.LogTrace("Client validation successful");

            // validate the token request
            var form = (await context.Request.ReadFormAsync()).AsNameValueCollection();
            var requestResult = await _requestValidator.ValidateRequestAsync(form, client);

            if (requestResult.IsError)
            {
                return new RevocationErrorResult(requestResult.Error);
            }

            var success = false;
            
            // revoke tokens
            if (requestResult.TokenTypeHint == Constants.TokenTypeHints.AccessToken)
            {
                _logger.LogTrace("Hint was for access token");
                success = await RevokeAccessTokenAsync(requestResult.Token, client);
            }
            else if (requestResult.TokenTypeHint == Constants.TokenTypeHints.RefreshToken)
            {
                _logger.LogTrace("Hint was for refresh token");
                success = await RevokeRefreshTokenAsync(requestResult.Token, client);
            }
            else
            {
                _logger.LogTrace("No hint for token type");

                success = await RevokeAccessTokenAsync(requestResult.Token, client);

                if (!success)
                {
                    success = await RevokeRefreshTokenAsync(requestResult.Token, client);
                    requestResult.TokenTypeHint = Constants.TokenTypeHints.RefreshToken;
                }
                else
                {
                    requestResult.TokenTypeHint = Constants.TokenTypeHints.AccessToken;
                }
            }

            if (success)
            {
                _logger.LogInformation("Token successfully revoked");
                await _events.RaiseAsync(new TokenRevokedSuccessEvent(requestResult, client));
            }
            else
            {
                _logger.LogInformation("No matching token found");
            }

            return new StatusCodeResult(HttpStatusCode.OK);
        }

        // revoke access token only if it belongs to client doing the request
        private async Task<bool> RevokeAccessTokenAsync(string handle, Client client)
        {
            var token = await _referenceTokenStore.GetReferenceTokenAsync(handle);

            if (token != null)
            {
                if (token.ClientId == client.ClientId)
                {
                    _logger.LogDebug("Access token revoked");
                    await _referenceTokenStore.RemoveReferenceTokenAsync(handle);
                }
                else
                {
                    _logger.LogWarning("Client {clientId} tried to revoke an access token belonging to a different client: {clientId}", client.ClientId, token.ClientId);
                }

                return true;
            }

            return false;
        }

        // revoke refresh token only if it belongs to client doing the request
        private async Task<bool> RevokeRefreshTokenAsync(string handle, Client client)
        {
            var token = await _refreshTokenStore.GetRefreshTokenAsync(handle);

            if (token != null)
            {
                if (token.ClientId == client.ClientId)
                {
                    _logger.LogDebug("Refresh token revoked");
                    await _refreshTokenStore.RemoveRefreshTokensAsync(token.SubjectId, token.ClientId);
                    await _referenceTokenStore.RemoveReferenceTokensAsync(token.SubjectId, token.ClientId);
                }
                else
                {
                    _logger.LogWarning("Client {clientId} tried to revoke a refresh token belonging to a different client: {clientId}", client.ClientId, token.ClientId);
                }

                return true;
            }

            return false;
        }
    }
}