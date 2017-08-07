// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Specialized;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Services;

namespace IdentityServer4.Endpoints
{
    class EndSessionEndpoint : IEndpointHandler
    {
        private readonly IEndSessionRequestValidator _endSessionRequestValidator;
        private readonly ILogger _logger;
        private readonly IUserSession _userSession;

        public EndSessionEndpoint(
            IEndSessionRequestValidator endSessionRequestValidator,
            IUserSession userSession,
            ILogger<EndSessionEndpoint> logger)
        {
            _endSessionRequestValidator = endSessionRequestValidator;
            _userSession = userSession;
            _logger = logger;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (context.Request.Path == Constants.ProtocolRoutePaths.EndSession.EnsureLeadingSlash())
            {
                return await ProcessSignoutAsync(context);
            }

            if (context.Request.Path == Constants.ProtocolRoutePaths.EndSessionCallback.EnsureLeadingSlash())
            {
                return await ProcessSignoutCallbackAsync(context);
            }

            _logger.LogWarning("Invalid request path to end session endpoint");
            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        private async Task<IEndpointResult> ProcessSignoutAsync(HttpContext context)
        {
            NameValueCollection parameters;
            if (context.Request.Method == "GET")
            {
                parameters = context.Request.Query.AsNameValueCollection();
            }
            else if (context.Request.Method == "POST")
            {
                parameters = (await context.Request.ReadFormAsync()).AsNameValueCollection();
            }
            else
            {
                _logger.LogWarning("Invalid HTTP method for end session endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var user = await _userSession.GetIdentityServerUserAsync();

            _logger.LogDebug("Processing signout request for {subjectId}", user?.GetSubjectId() ?? "anonymous");

            var result = await _endSessionRequestValidator.ValidateAsync(parameters, user);
            
            if (result.IsError)
            {
                _logger.LogError("Error processing end session request {error}", result.Error);
            }
            else
            {
                _logger.LogDebug("Success validating end session request from {clientId}", result.ValidatedRequest?.Client?.ClientId);
            }

            return new EndSessionResult(result);
        }

        private async Task<IEndpointResult> ProcessSignoutCallbackAsync(HttpContext context)
        {
            if (context.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for end session callback endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogDebug("Processing signout callback request");

            var parameters = context.Request.Query.AsNameValueCollection();
            var result = await _endSessionRequestValidator.ValidateCallbackAsync(parameters);

            if (result.IsError == false)
            {
                _logger.LogInformation("Successful signout callback. Client logout iframe urls: {urls}", result.FrontChannelLogoutUrls);
            }

            return new EndSessionCallbackResult(result);
        }
    }
}
