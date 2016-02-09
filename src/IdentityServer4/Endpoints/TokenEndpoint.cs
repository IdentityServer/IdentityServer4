// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Endpoints.Results;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Validation;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
{
    public class TokenEndpoint : IEndpoint
    {
        private readonly ClientSecretValidator _clientValidator;
        private readonly ILogger _logger;
        private readonly ITokenRequestValidator _requestValidator;
        private readonly ITokenResponseGenerator _responseGenerator;

        public TokenEndpoint(ITokenRequestValidator requestValidator, ClientSecretValidator clientValidator, ITokenResponseGenerator responseGenerator, ILoggerFactory loggerFactory)
        {
            _requestValidator = requestValidator;
            _clientValidator = clientValidator;
            _responseGenerator = responseGenerator;
            _logger = loggerFactory.CreateLogger<TokenEndpoint>();
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            _logger.LogVerbose("Start token request.");

            // validate HTTP
            if (context.HttpContext.Request.Method != "POST" || !context.HttpContext.Request.HasFormContentType)
            {
                // todo logging
                return new TokenErrorResult(OidcConstants.TokenErrors.InvalidRequest);
            }

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
            return new TokenResult(response);
        }
    }
}