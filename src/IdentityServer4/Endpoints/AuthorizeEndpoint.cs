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
using Microsoft.Extensions.WebEncoders;

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
        private readonly IAuthorizationResultGenerator _resultGenerator;
        private readonly IUrlEncoder _urlEncoder;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerContext context,
            IAuthorizeResponseGenerator responseGenerator,
            IAuthorizeRequestValidator validator,
            IAuthorizeInteractionResponseGenerator interactionGenerator,
            IAuthorizationResultGenerator resultGenerator,
            IUrlEncoder urlEncoder)
        {
            _events = events;
            _logger = logger;
            _context = context;
            _responseGenerator = responseGenerator;
            _validator = validator;
            _interactionGenerator = interactionGenerator;
            _resultGenerator = resultGenerator;
            _urlEncoder = urlEncoder;
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Method != "GET")
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogInformation("Start Authorize Request");

            var values = context.HttpContext.Request.Query.AsNameValueCollection();
            var user = await _context.GetIdentityServerUserAsync();
            var result = await ProcessAuthorizeRequestAsync(values, user, null);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IEndpointResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, UserConsentResponseMessage consent)
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

        private async Task<IEndpointResult> SuccessfulAuthorizationAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateResponseAsync(request);
            var result = await _resultGenerator.CreateAuthorizeResultAsync(response);

            await RaiseSuccessEventAsync();

            return result;
        }

        async Task<IEndpointResult> LoginAsync(SignInMessage message, NameValueCollection parameters)
        {
            var url = _context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Constants.RoutePaths.Oidc.Authorize;
            url.AddQueryString(parameters.ToQueryString(_urlEncoder));
            message.ReturnUrl = url;

            return await _resultGenerator.CreateLoginResultAsync(message);
        }

        private async Task<IEndpointResult> ConsentAsync(ValidatedAuthorizeRequest validatedRequest, UserConsentResponseMessage consent, NameValueCollection requestParameters, string errorMessage)
        {
            return await _resultGenerator.CreateConsentResultAsync();
        }

        async Task<IEndpointResult> ErrorAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
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