using System;
using System.Collections.Generic;
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
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Events;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.ViewModels;

namespace IdentityServer4.Core.Endpoints
{
    class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;
        private readonly IdentityServerContext _context;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly ILocalizationService _localizationService;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerContext context,
            IAuthorizeRequestValidator validator,
            ILocalizationService localizationService)
        {
            _events = events;
            _logger = logger;
            _context = context;
            _validator = validator;
            _localizationService = localizationService;
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
            var result = await ProcessRequestAsync(values, user);

            _logger.LogInformation("End Authorize Request. Result type: {0}", result?.GetType().ToString() ?? "-none-");

            return result;
        }

        internal async Task<IResult> ProcessRequestAsync(NameValueCollection parameters, ClaimsPrincipal user)
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
                return await this.AuthorizeErrorAsync(result);
            }

            return null;
        }

        async Task<IResult> AuthorizeErrorAsync(AuthorizeRequestValidationResult result)
        {
            await RaiseFailureEventAsync(result.Error);

            // show error message to user
            if (result.ErrorType == ErrorTypes.User)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = _context.GetRequestId(),
                    ErrorCode = result.Error,
                    ErrorMessage = LookupErrorMessage(result.Error)
                };
                return new ErrorPageResult(errorModel);
            }

            return new AuthorizeResult();

            //// return error to client
            //var response = new AuthorizeResponse
            //{
            //    Request = request,

            //    IsError = true,
            //    Error = error,
            //    State = request.State,
            //    RedirectUri = request.RedirectUri
            //};

            //if (request.ResponseMode == Constants.ResponseModes.FormPost)
            //{
            //    return new AuthorizeFormPostResult(response, Request);
            //}
            //else
            //{
            //    return new AuthorizeRedirectResult(response, _options);
            //}
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.Authorize, error);
        }

        private string LookupErrorMessage(string error)
        {
            var msg = _localizationService.GetMessage(error);
            if (msg.IsMissing())
            {
                msg = error;
            }
            return msg;
        }
    }
}