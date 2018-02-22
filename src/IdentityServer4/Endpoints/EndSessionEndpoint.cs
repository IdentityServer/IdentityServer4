// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Endpoints
{
    internal class EndSessionEndpoint : IEndpointHandler
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
            NameValueCollection parameters;
            if (HttpMethods.IsGet(context.Request.Method))
            {
                parameters = context.Request.Query.AsNameValueCollection();
            }
            else if (HttpMethods.IsPost(context.Request.Method))
            {
                parameters = (await context.Request.ReadFormAsync()).AsNameValueCollection();
            }
            else
            {
                _logger.LogWarning("Invalid HTTP method for end session endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var user = await _userSession.GetUserAsync();

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
    }
}
