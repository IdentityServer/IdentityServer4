// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net;
using System.Security.Claims;
using IdentityServer4.Hosting;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Stores;
using IdentityModel;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Endpoints
{
    class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IAuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IMessageStore<ConsentResponse> _consentResponseStore;
        private readonly IAuthorizeResponseGenerator _authorizeResponseGenerator;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IMessageStore<ConsentResponse> consentResponseStore,
            IAuthorizeResponseGenerator authorizeResponseGenerator)
        {
            _events = events;
            _logger = logger;
            _validator = validator;
            _interactionGenerator = interactionGenerator;
            _consentResponseStore = consentResponseStore;
            _authorizeResponseGenerator = authorizeResponseGenerator;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (context.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for authorize endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (context.Request.Path == Constants.ProtocolRoutePaths.Authorize.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAsync(context);
            }

            if (context.Request.Path == Constants.ProtocolRoutePaths.AuthorizeAfterLogin.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAfterLoginAsync(context);
            }

            if (context.Request.Path == Constants.ProtocolRoutePaths.AuthorizeAfterConsent.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAfterConsentAsync(context);
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        async Task<IEndpointResult> ProcessAuthorizeAsync(HttpContext context)
        {
            _logger.LogDebug("Start authorize request");

            var values = context.Request.Query.AsNameValueCollection();
            var user = await context.GetIdentityServerUserAsync();

            var result = await ProcessAuthorizeRequestAsync(values, user, null);

            _logger.LogInformation("End authorize request. result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeAfterLoginAsync(HttpContext context)
        {
            _logger.LogDebug("Start authorize request (after login)");

            var user = await context.GetIdentityServerUserAsync();
            if (user == null)
            {
                _logger.LogError("User is not authenticated.");
                return await ErrorPageAsync(null, OidcConstants.AuthorizeErrors.ServerError);
            }

            var parameters = context.Request.Query.AsNameValueCollection();
            var result = await ProcessAuthorizeRequestAsync(parameters, user, null);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeAfterConsentAsync(HttpContext context)
        {
            _logger.LogInformation("Start authorize request (after consent)");

            var user = await context.GetIdentityServerUserAsync();
            if (user == null)
            {
                _logger.LogError("User is not authenticated.");
                return await ErrorPageAsync(null, OidcConstants.AuthorizeErrors.ServerError);
            }

            var parameters = context.Request.Query.AsNameValueCollection();
            var consentRequest = new ConsentRequest(parameters, user.GetSubjectId());

            var consent = await _consentResponseStore.ReadAsync(consentRequest.Id);
            if (consent == null)
            {
                _logger.LogError("consent message is missing.");
                return await ErrorPageAsync(null, OidcConstants.AuthorizeErrors.ServerError);
            }
            if (consent.Data == null)
            {
                _logger.LogError("consent message is missing Consent data.");
                return await ErrorPageAsync(null, OidcConstants.AuthorizeErrors.ServerError);
            }

            var result = await ProcessAuthorizeRequestAsync(parameters, user, consent.Data);
            await _consentResponseStore.DeleteAsync(consentRequest.Id);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, ConsentResponse consent)
        {
            if (user != null)
            {
                _logger.LogTrace("User in authorize request: name:{name}, sub:{sub}", user.GetName(), user.GetSubjectId());
            }
            else
            {
                _logger.LogTrace("No user present in authorize request");
            }

            // validate request
            var result = await _validator.ValidateAsync(parameters, user);
            if (result.IsError)
            {
                return await ErrorPageAsync(
                    result.ValidatedRequest,
                    result.Error,
                    result.ErrorDescription);
            }

            var request = result.ValidatedRequest;

            // determine user interaction
            var interactionResult = await _interactionGenerator.ProcessInteractionAsync(request, consent);
            if (interactionResult.IsError)
            {
                return await ErrorPageAsync(
                    request,
                    interactionResult.Error.Error);
            }
            if (interactionResult.IsLogin)
            {
                _logger.LogDebug("Showing login page");
                return new LoginPageResult(request);
            }
            if (interactionResult.IsConsent)
            {
                _logger.LogDebug("Showing consent page");
                return new ConsentPageResult(request);
            }


            var response = await _authorizeResponseGenerator.CreateResponseAsync(request);

            // issue response
            _logger.LogInformation("Issuing successful authorization response");
            await RaiseSuccessEventAsync();
            return new AuthorizeResult(response);
        }

        async Task<IEndpointResult> ErrorPageAsync(ValidatedAuthorizeRequest request, string error, string description = null)
        {
            _logger.LogDebug("Showing error page");

            await RaiseFailureEventAsync(error);

            return new AuthorizeErrorResult(request, error, description);
        }

        private async Task RaiseSuccessEventAsync()
        {
            await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.Authorize);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.Authorize, error);
        }
   }
}