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

namespace IdentityServer4.Core.Endpoints
{
    class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;
        private readonly IdentityServerContext _context;
        private readonly AuthorizeRequestValidator _validator;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerContext context,
            AuthorizeRequestValidator validator)
        {
            _events = events;
            _logger = logger;
            _context = context;
            _validator = validator;
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

        public async Task<IResult> ProcessRequestAsync(NameValueCollection parameters, ClaimsPrincipal user)
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
                return await this.AuthorizeErrorAsync(
                    result.ErrorType,
                    result.Error,
                    result.ValidatedRequest);
            }

            return null;
        }

        async Task<IResult> AuthorizeErrorAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            await RaiseFailureEventAsync(error);

            return new ErrorPageResult(error);

            // show error message to user
            if (errorType == ErrorTypes.User)
            {
                //var env = Request.GetOwinEnvironment();
                //var errorModel = new ErrorViewModel
                //{
                //    RequestId = env.GetRequestId(),
                //    SiteName = _options.SiteName,
                //    SiteUrl = env.GetIdentityServerBaseUrl(),
                //    CurrentUser = env.GetCurrentUserDisplayName(),
                //    LogoutUrl = env.GetIdentityServerLogoutUrl(),
                //    ErrorMessage = LookupErrorMessage(error)
                //};

                //var errorResult = new ErrorActionResult(_viewService, errorModel);
                return null;
            }

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
    }
}