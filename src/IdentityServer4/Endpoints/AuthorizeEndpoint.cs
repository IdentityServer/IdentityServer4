// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Extensions;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net;
using System.Security.Claims;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Events;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Resources;
using IdentityServer4.Core.Endpoints.Results;

namespace IdentityServer4.Core.Endpoints
{
    class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;
        private readonly IdentityServerContext _context;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IAuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IAuthorizeEndpointResultFactory _resultGenerator;
        private readonly IMessageStore<SignInResponse> _signInResponseStore;
        private readonly IMessageStore<ConsentResponse> _consentResponseStore;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerContext context,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeEndpointResultFactory resultGenerator,
            IMessageStore<SignInResponse> signInResponseStore,
            IMessageStore<ConsentResponse> consentRequestStore)
        {
            _events = events;
            _logger = logger;
            _context = context;
            _validator = validator;
            _interactionGenerator = interactionGenerator;
            _resultGenerator = resultGenerator;
            _signInResponseStore = signInResponseStore;
            _consentResponseStore = consentRequestStore;
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Method != "GET")
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (context.HttpContext.Request.Path == Constants.RoutePaths.Oidc.Authorize.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAsync(context);
            }

            if (context.HttpContext.Request.Path == Constants.RoutePaths.Oidc.AuthorizeAfterLogin.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAfterLoginAsync(context);
            }

            if (context.HttpContext.Request.Path == Constants.RoutePaths.Oidc.AuthorizeAfterConsent.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAfterConsentAsync(context);
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        async Task<IEndpointResult> ProcessAuthorizeAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Start Authorize Request");

            var values = context.HttpContext.Request.Query.AsNameValueCollection();
            var user = await _context.GetIdentityServerUserAsync();

            var result = await ProcessAuthorizeRequestAsync(values, user, null);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeAfterLoginAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Start Authorize Request (after login)");

            if (!context.HttpContext.Request.Query.ContainsKey("id"))
            {
                _logger.LogWarning("id query parameter is missing.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }

            var id = context.HttpContext.Request.Query["id"].First();
            var message = await _signInResponseStore.ReadAsync(id);
            if (message == null)
            {
                _logger.LogWarning("signin message is missing.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }
            if (message.AuthorizeRequestParameters == null)
            {
                _logger.LogWarning("signin message is missing AuthorizeRequestParameters data.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }

            var user = await _context.GetIdentityServerUserAsync();

            var result = await ProcessAuthorizeRequestAsync(message.AuthorizeRequestParameters.ToNameValueCollection(), user, null);

            await _signInResponseStore.DeleteAsync(id);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeAfterConsentAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Start Authorize Request (after consent)");

            if (!context.HttpContext.Request.Query.ContainsKey("id"))
            {
                _logger.LogWarning("id query parameter is missing.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }

            var id = context.HttpContext.Request.Query["id"].First();
            var consent = await _consentResponseStore.ReadAsync(id);
            if (consent == null)
            {
                _logger.LogWarning("consent message is missing.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }
            if (consent.AuthorizeRequestParameters == null)
            {
                _logger.LogWarning("consent message is missing AuthorizeRequestParameters data.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }
            if (consent.Data == null)
            {
                _logger.LogWarning("consent message is missing Consent data.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }

            var user = await _context.GetIdentityServerUserAsync();

            var result = await ProcessAuthorizeRequestAsync(consent.AuthorizeRequestParameters.ToNameValueCollection(), user, consent.Data);

            await _consentResponseStore.DeleteAsync(id);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, ConsentResponse consent)
        {
            if (user != null)
            {
                _logger.LogVerbose("User in authorize request: name:{0}, sub:{1}", user.GetName(), user.GetSubjectId());
            }
            else
            {
                _logger.LogVerbose("No user present in authorize request");
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
            _logger.LogInformation("Showing login page");
            return await _resultGenerator.CreateLoginResultAsync(request);
        }

        private async Task<IEndpointResult> ConsentPageAsync(ValidatedAuthorizeRequest validatedRequest)
        {
            _logger.LogInformation("Showing consent page");
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
            _logger.LogInformation("Showing error page");

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