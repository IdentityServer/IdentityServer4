using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Results;
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
using Microsoft.Extensions.WebEncoders;
using IdentityServer4.Core.Resources;

namespace IdentityServer4.Core.Endpoints
{
    class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;
        private readonly IdentityServerContext _context;
        private readonly IAuthorizeResponseGenerator _responseGenerator;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IAuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IAuthorizeEndpointResultGenerator _resultGenerator;
        private readonly IMessageStore<UserConsentResponseMessage> _consentMessageStore;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerContext context,
            IAuthorizeResponseGenerator responseGenerator,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizeEndpointResultGenerator resultGenerator,
            IMessageStore<UserConsentResponseMessage> consentMessageStore)
        {
            _events = events;
            _logger = logger;
            _context = context;
            _responseGenerator = responseGenerator;
            _validator = validator;
            _interactionGenerator = interactionGenerator;
            _resultGenerator = resultGenerator;
            _consentMessageStore = consentMessageStore;
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

            if (context.HttpContext.Request.Path == Constants.RoutePaths.Oidc.AuthorizeWithConsent.EnsureLeadingSlash())
            {
                return await ProcessAuthorizeWithConsentAsync(context);
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

        internal async Task<IEndpointResult> ProcessAuthorizeWithConsentAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Start Authorize Request (with consent)");

            if (!context.HttpContext.Request.Query.ContainsKey("id"))
            {
                _logger.LogWarning("id query parameter is missing.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }

            var id = context.HttpContext.Request.Query["id"].First();
            var consent = await _consentMessageStore.ReadAsync(id);
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
            if (consent.Consent == null)
            {
                _logger.LogWarning("consent message is missing Consent data.");
                return await ErrorPageAsync(ErrorTypes.User, nameof(Messages.UnexpectedError), null);
            }

            var user = await _context.GetIdentityServerUserAsync();

            var result = await ProcessAuthorizeRequestAsync(consent.AuthorizeRequestParameters, user, consent.Consent);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, UserConsent consent)
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

            var loginInteraction = await _interactionGenerator.ProcessLoginAsync(request, user);
            if (loginInteraction.IsError)
            {
                return await ErrorPageAsync(
                    loginInteraction.Error.ErrorType,
                    loginInteraction.Error.Error,
                    request);
            }
            if (loginInteraction.IsLogin)
            {
                return await LoginPageAsync(request);
            }

            // user must be authenticated at this point
            if (!user.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            var consentInteraction = await _interactionGenerator.ProcessConsentAsync(request, consent);
            if (consentInteraction.IsError)
            {
                return await ErrorPageAsync(
                    consentInteraction.Error.ErrorType,
                    consentInteraction.Error.Error,
                    request);
            }
            if (consentInteraction.IsConsent)
            {
                _logger.LogInformation("Showing consent screen");
                return await ConsentPageAsync(request);
            }

            return await SuccessfulAuthorizationAsync(request);
        }

        private async Task<IEndpointResult> SuccessfulAuthorizationAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateResponseAsync(request);
            var result = await _resultGenerator.CreateAuthorizeResultAsync(response);

            await RaiseSuccessEventAsync();

            return result;
        }

        async Task<IEndpointResult> LoginPageAsync(ValidatedAuthorizeRequest request)
        {
            return await _resultGenerator.CreateLoginResultAsync(request);
        }

        private async Task<IEndpointResult> ConsentPageAsync(ValidatedAuthorizeRequest validatedRequest)
        {
            return await _resultGenerator.CreateConsentResultAsync(validatedRequest);
        }

        async Task<IEndpointResult> ErrorPageAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            await RaiseFailureEventAsync(error);

            return await _resultGenerator.CreateErrorResultAsync(
                errorType,
                error,
                request);
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