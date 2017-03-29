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
using IdentityServer4.Logging;
using System;

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
            if (context.Request.Path == Constants.ProtocolRoutePaths.Authorize.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeAsync(context);
            }

            if (context.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for authorize endpoint.");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
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

            NameValueCollection values;

            if (context.Request.Method == "GET")
            {
                values = context.Request.Query.AsNameValueCollection();
            }
            else if (context.Request.Method == "POST")
            {
                if (!context.Request.HasFormContentType)
                {
                    return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
                }

                values = context.Request.Form.AsNameValueCollection();
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var user = await context.GetIdentityServerUserAsync();
            var result = await ProcessAuthorizeRequestAsync(values, user, null);

            _logger.LogTrace("End authorize request. result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeAfterLoginAsync(HttpContext context)
        {
            _logger.LogDebug("Start authorize request (after login)");

            var user = await context.GetIdentityServerUserAsync();
            if (user == null)
            {
                return await CreateErrorResultAsync("User is not authenticated");
            }

            var parameters = context.Request.Query.AsNameValueCollection();
            var result = await ProcessAuthorizeRequestAsync(parameters, user, null);

            _logger.LogTrace("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeAfterConsentAsync(HttpContext context)
        {
            _logger.LogDebug("Start authorize request (after consent)");

            var user = await context.GetIdentityServerUserAsync();
            if (user == null)
            {
                return await CreateErrorResultAsync("User is not authenticated");
            }

            var parameters = context.Request.Query.AsNameValueCollection();
            var consentRequest = new ConsentRequest(parameters, user.GetSubjectId());

            var consent = await _consentResponseStore.ReadAsync(consentRequest.Id);
            if (consent == null)
            {
                return await CreateErrorResultAsync("consent message is missing");
            }

            try
            { 
                if (consent.Data == null)
                {
                    return await CreateErrorResultAsync("consent message is missing data");
                }

                var result = await ProcessAuthorizeRequestAsync(parameters, user, consent.Data);

                _logger.LogTrace("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

                return result;
            }
            finally
            {
                await _consentResponseStore.DeleteAsync(consentRequest.Id);
            }
        }

        internal async Task<IEndpointResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, ConsentResponse consent)
        {
            if (user != null)
            {
                _logger.LogDebug("User in authorize request: {subjectId}", user.GetSubjectId());
            }
            else
            {
                _logger.LogDebug("No user present in authorize request");
            }

            // validate request
            var result = await _validator.ValidateAsync(parameters, user);
            if (result.IsError)
            {
                return await CreateErrorResultAsync(
                    "Request validation failed",
                    result.ValidatedRequest,
                    result.Error,
                    result.ErrorDescription);
            }

            var request = result.ValidatedRequest;
            LogRequest(request);

            // determine user interaction
            var interactionResult = await _interactionGenerator.ProcessInteractionAsync(request, consent);
            if (interactionResult.IsError)
            {
                return await CreateErrorResultAsync("Interaction generator error", request, interactionResult.Error, logError: false);
            }
            if (interactionResult.IsLogin)
            {
                return new LoginPageResult(request);
            }
            if (interactionResult.IsConsent)
            {
                return new ConsentPageResult(request);
            }
            if (interactionResult.IsRedirect)
            {
                return new CustomRedirectResult(request, interactionResult.RedirectUrl);
            }

            var response = await _authorizeResponseGenerator.CreateResponseAsync(request);

            await RaiseResponseEventAsync(response);

            LogResponse(response);
            
            return new AuthorizeResult(response);
        }

        private void LogRequest(ValidatedAuthorizeRequest request)
        {
            var details = new AuthorizeRequestValidationLog(request);
            _logger.LogInformation("ValidatedAuthorizeRequest" + Environment.NewLine + "{validationDetails}", details);
        }

        private void LogResponse(AuthorizeResponse response)
        {
            var details = new AuthorizeResponseLog(response);
            _logger.LogInformation("Authorize endpoint response" + Environment.NewLine + "{response}", details);
        }

        async Task<IEndpointResult> CreateErrorResultAsync(
            string logMessage,
            ValidatedAuthorizeRequest request = null, 
            string error = OidcConstants.AuthorizeErrors.ServerError, 
            string errorDescription = null, 
            bool logError = true)
        {
            if (logError)
            {
                _logger.LogError(logMessage);
            }

            if (request != null)
            {
                var details = new AuthorizeRequestValidationLog(request);
                _logger.LogInformation("{validationDetails}", details);
            }

            // TODO: should we raise a token failure event for all errors to the authorize endpoint?
            await RaiseFailureEventAsync(request, error, errorDescription);

            return new AuthorizeResult(new AuthorizeResponse {
                Request = request,
                Error = error,
                ErrorDescription = errorDescription
            });
        }

        private Task RaiseResponseEventAsync(AuthorizeResponse response)
        {
            if (!response.IsError)
            {
                LogTokens(response);
                return _events.RaiseAsync(new TokenIssuedSuccessEvent(response));
            }
            else
            {
                return RaiseFailureEventAsync(response.Request, response.Error, response.ErrorDescription);
            }
        }

        private Task RaiseFailureEventAsync(ValidatedAuthorizeRequest request, string error, string errorDescription)
        {
            return _events.RaiseAsync(new TokenIssuedFailureEvent(request, error, errorDescription));
        }

        private void LogTokens(AuthorizeResponse response)
        {
            var clientId = $"{response.Request.ClientId} ({response.Request.Client.ClientName ?? "no name set"})";
            var subjectId = response.Request.Subject.GetSubjectId();

            if (response.IdentityToken != null)
            {
                _logger.LogTrace("Identity token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.IdentityToken);
            }
            if (response.Code != null)
            {
                _logger.LogTrace("Code issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.Code);
            }
            if (response.AccessToken != null)
            {
                _logger.LogTrace("Access token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.AccessToken);
            }
        }
    }
}