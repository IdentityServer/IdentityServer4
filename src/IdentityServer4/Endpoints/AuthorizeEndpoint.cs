// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Microsoft.AspNet.Http;
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
using IdentityServer4.Resources;
using IdentityServer4.Endpoints.Results;

namespace IdentityServer4.Endpoints
{
    class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;
        private readonly IdentityServerContext _context;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IAuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IAuthorizeEndpointResultFactory _resultGenerator;
        private readonly IMessageStore<ConsentResponse> _consentResponseStore;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerContext context,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeEndpointResultFactory resultGenerator,
            IMessageStore<ConsentResponse> consentResponseStore)
        {
            _events = events;
            _logger = logger;
            _context = context;
            _validator = validator;
            _interactionGenerator = interactionGenerator;
            _resultGenerator = resultGenerator;
            _consentResponseStore = consentResponseStore;
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for authorize endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (context.HttpContext.Request.Path == Constants.ProtocolRoutePaths.Authorize.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAsync(context);
            }

            if (context.HttpContext.Request.Path == Constants.ProtocolRoutePaths.AuthorizeAfterLogin.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAfterLoginAsync(context);
            }

            if (context.HttpContext.Request.Path == Constants.ProtocolRoutePaths.AuthorizeAfterConsent.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAfterConsentAsync(context);
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        async Task<IEndpointResult> ProcessAuthorizeAsync(IdentityServerContext context)
        {
            _logger.LogDebug("Start authorize request");

            var values = context.HttpContext.Request.Query.AsNameValueCollection();
            var user = await _context.GetIdentityServerUserAsync();

            var result = await ProcessAuthorizeRequestAsync(values, user, null);

            _logger.LogInformation("End authorize request. result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeAfterLoginAsync(IdentityServerContext context)
        {
            _logger.LogDebug("Start authorize request (after login)");

            var user = await _context.GetIdentityServerUserAsync();
            if (user == null)
            {
                _logger.LogError("User is not authenticated.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }

            var parameters = context.HttpContext.Request.Query.AsNameValueCollection();
            var result = await ProcessAuthorizeRequestAsync(parameters, user, null);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeAfterConsentAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Start authorize request (after consent)");

            var user = await _context.GetIdentityServerUserAsync();
            if (user == null)
            {
                _logger.LogError("User is not authenticated.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }

            var parameters = context.HttpContext.Request.Query.AsNameValueCollection();
            var consentRequest = new ConsentRequest(parameters, user.GetSubjectId());

            var consent = await _consentResponseStore.ReadAsync(consentRequest.Id);
            if (consent == null)
            {
                _logger.LogError("consent message is missing.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }
            if (consent.Data == null)
            {
                _logger.LogError("consent message is missing Consent data.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
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
                    result.ErrorType, 
                    result.Error, 
                    result.ValidatedRequest);
            }

            var request = result.ValidatedRequest;

            // determine user interaction
            var interactionResult = await _interactionGenerator.ProcessInteractionAsync(request, consent);
            if (interactionResult.IsError)
            {
                return await ErrorPageAsync(
                    interactionResult.Error.ErrorType,
                    interactionResult.Error.Error,
                    request);
            }
            if (interactionResult.IsLogin)
            {
                return await LoginPageAsync(request);
            }
            if (interactionResult.IsConsent)
            {
                return await ConsentPageAsync(request);
            }

            // issue response
            return await SuccessfulAuthorizationAsync(request);
        }

        async Task<IEndpointResult> LoginPageAsync(ValidatedAuthorizeRequest request)
        {
            _logger.LogDebug("Showing login page");
            return await _resultGenerator.CreateLoginResultAsync(request);
        }

        private async Task<IEndpointResult> ConsentPageAsync(ValidatedAuthorizeRequest validatedRequest)
        {
            _logger.LogDebug("Showing consent page");
            return await _resultGenerator.CreateConsentResultAsync(validatedRequest);
        }

        private async Task<IEndpointResult> SuccessfulAuthorizationAsync(ValidatedAuthorizeRequest request)
        {
            _logger.LogInformation("Issuing successful authorization response");

            var result = await _resultGenerator.CreateAuthorizeResultAsync(request);

            await RaiseSuccessEventAsync();

            return result;
        }

        async Task<IEndpointResult> ErrorPageAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            _logger.LogDebug("Showing error page");

            var result = await _resultGenerator.CreateErrorResultAsync(
                errorType,
                error,
                request);

            await RaiseFailureEventAsync(error);

            return result;
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