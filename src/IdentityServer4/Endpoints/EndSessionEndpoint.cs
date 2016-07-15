// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Http;
using System.Collections.Specialized;
using IdentityServer4.Validation;

namespace IdentityServer4.Endpoints
{
    class EndSessionEndpoint : IEndpoint
    {
        private readonly ILogger<EndSessionEndpoint> _logger;
        private readonly IdentityServerContext _context;
        private readonly IEndSessionRequestValidator _endSessionRequestValidator;
        private readonly ClientListCookie _clientListCookie;

        public EndSessionEndpoint(
            ILogger<EndSessionEndpoint> logger, 
            IdentityServerContext context,
            IEndSessionRequestValidator endSessionRequestValidator,
            ClientListCookie clientListCookie)
        {
            _logger = logger;
            _context = context;
            _endSessionRequestValidator = endSessionRequestValidator;
            _clientListCookie = clientListCookie;
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Path == Constants.RoutePaths.Oidc.EndSession.EnsureLeadingSlash())
            {
                return await ProcessSignoutAsync(context);
            }

            if (context.HttpContext.Request.Path == Constants.RoutePaths.Oidc.EndSessionCallback.EnsureLeadingSlash())
            {
                return await ProcessSignoutCallbackAsync(context);
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        private async Task<IEndpointResult> ProcessSignoutAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Processing singout request");

            NameValueCollection parameters = null;
            if (context.HttpContext.Request.Method == "GET")
            {
                parameters = context.HttpContext.Request.Query.AsNameValueCollection();
            }
            else if (context.HttpContext.Request.Method == "POST")
            {
                parameters = context.HttpContext.Request.Form.AsNameValueCollection();
            }
            else
            {
                _logger.LogWarning("Invalid HTTP method for end session endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var user = await _context.GetIdentityServerUserAsync();
            var result = await _endSessionRequestValidator.ValidateAsync(parameters, user);
            if (result.IsError)
            {
                // if anything went wrong, ignore the params the RP sent
                return new LogoutPageResult(_context.Options.UserInteractionOptions);
            }
            
            return new LogoutPageResult(_context.Options.UserInteractionOptions);
        }

        private Task<IEndpointResult> ProcessSignoutCallbackAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for end session endpoint.");
                return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
            }

            _logger.LogInformation("Processing singout callback request");

            return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.NoContent));
        }
    }
}