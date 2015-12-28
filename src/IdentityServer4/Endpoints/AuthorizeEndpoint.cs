using System;
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
        private readonly IResultGenerator _resultGenerator;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerContext context,
            IAuthorizeResponseGenerator responseGenerator,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IResultGenerator resultGenerator)
        {
            _events = events;
            _logger = logger;
            _context = context;
            _responseGenerator = responseGenerator;
            _validator = validator;
            _interactionGenerator = interactionGenerator;
            _resultGenerator = resultGenerator;
        }

        public async Task<IResult> ProcessAsync(HttpContext context)
        {
            if (context.Request.Method != "GET")
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogInformation("Start Authorize Request");

            var values = context.Request.Query.AsNameValueCollection();
            var user = await _context.GetIdentityServerUserAsync();
            var result = await ProcessAuthorizeRequestAsync(values, user, null);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, UserConsentResponseMessage consent)
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
                return await ErrorAsync(
                    result.ErrorType, 
                    result.Error, 
                    result.ValidatedRequest);
            }

            var request = result.ValidatedRequest;

            var loginInteraction = await _interactionGenerator.ProcessLoginAsync(request, user);
            if (loginInteraction.IsError)
            {
                return await ErrorAsync(
                    loginInteraction.Error.ErrorType,
                    loginInteraction.Error.Error,
                    request);
            }
            if (loginInteraction.IsLogin)
            {
                return await LoginAsync(loginInteraction.SignInMessage, request.Raw);
            }

            // user must be authenticated at this point
            if (!user.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            request.Subject = user;

            // now that client configuration is loaded, we can do further validation
            loginInteraction = await _interactionGenerator.ProcessClientLoginAsync(request);
            if (loginInteraction.IsLogin)
            {
                return await LoginAsync(loginInteraction.SignInMessage, request.Raw);
            }

            var consentInteraction = await _interactionGenerator.ProcessConsentAsync(request, consent);
            if (consentInteraction.IsError)
            {
                return await ErrorAsync(
                    consentInteraction.Error.ErrorType,
                    consentInteraction.Error.Error,
                    request);
            }
            if (consentInteraction.IsConsent)
            {
                _logger.LogInformation("Showing consent screen");
                return await ConsentAsync(request, consent, request.Raw, consentInteraction.ConsentError);
            }

            return await SuccessfulAuthorizationAsync(request);
        }

        private async Task<IResult> SuccessfulAuthorizationAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateResponseAsync(request);
            var result = await _resultGenerator.CreateAuthorizeResultAsync(response);

            await RaiseSuccessEventAsync();

            return result;
        }

        async Task<IResult> LoginAsync(SignInMessage message, NameValueCollection parameters)
        {
            var url = _context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Constants.RoutePaths.Oidc.Authorize;
            url.AddQueryString(parameters.ToQueryString());
            message.ReturnUrl = url;

            return await _resultGenerator.CreateLoginResultAsync(message);
        }

        private async Task<IResult> ConsentAsync(ValidatedAuthorizeRequest validatedRequest, UserConsentResponseMessage consent, NameValueCollection requestParameters, string errorMessage)
        {
            return await _resultGenerator.CreateConsentResultAsync();
        }

        async Task<IResult> ErrorAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
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