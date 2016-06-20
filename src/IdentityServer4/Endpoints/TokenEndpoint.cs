// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Validation;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    public class TokenEndpoint : IEndpoint
    {
        private readonly ClientSecretValidator _clientValidator;
        private readonly ILogger _logger;
        private readonly ITokenRequestValidator _requestValidator;
        private readonly ITokenResponseGenerator _responseGenerator;

        public TokenEndpoint(ITokenRequestValidator requestValidator, ClientSecretValidator clientValidator, ITokenResponseGenerator responseGenerator, ILogger<TokenEndpoint> logger)
        {
            _requestValidator = requestValidator;
            _clientValidator = clientValidator;
            _responseGenerator = responseGenerator;
            _logger = logger;
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            _logger.LogTrace("Processing token request.");

            // validate HTTP
            if (context.HttpContext.Request.Method != "POST" || !context.HttpContext.Request.HasFormContentType)
            {
                _logger.LogWarning("Invalid HTTP request for token endpoint");
                return new TokenErrorResult(OidcConstants.TokenErrors.InvalidRequest);
            }

            return await ProcessTokenRequestAsync(context);
        }

        private async Task<IEndpointResult> ProcessTokenRequestAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Start token request.");

            // validate client
            var clientResult = await _clientValidator.ValidateAsync(context.HttpContext);

            if (clientResult.Client == null)
            {
                return new TokenErrorResult(OidcConstants.TokenErrors.InvalidClient);
            }

            // validate request
            var requestResult = await _requestValidator.ValidateRequestAsync(
                context.HttpContext.Request.Form.AsNameValueCollection(),
                clientResult.Client);

            if (requestResult.IsError)
            {
                return new TokenErrorResult(requestResult.Error, requestResult.ErrorDescription);
            }

            // create response
            var response = await _responseGenerator.ProcessAsync(requestResult.ValidatedRequest);

            // return result
            _logger.LogInformation("Token request success.");
            return new TokenResult(response);
        }
    }
}