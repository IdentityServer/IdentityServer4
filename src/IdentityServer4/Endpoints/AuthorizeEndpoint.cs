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
using Microsoft.Extensions.WebEncoders;

namespace IdentityServer4.Core.Endpoints
{
    class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;
        private readonly IdentityServerContext _context;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly ILocalizationService _localizationService;
        private readonly IHtmlEncoder _encoder;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerContext context,
            IAuthorizeRequestValidator validator,
            ILocalizationService localizationService,
            IHtmlEncoder encoder)
        {
            _events = events;
            _logger = logger;
            _context = context;
            _validator = validator;
            _localizationService = localizationService;
            _encoder = encoder;
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
                return await AuthorizeErrorAsync(result.ErrorType, result.Error, result.ValidatedRequest);
            }

            return null;
        }

        async Task<IResult> AuthorizeErrorAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            await RaiseFailureEventAsync(error);

            var errorModel = new ErrorViewModel
            {
                RequestId = _context.GetRequestId(),
                ErrorCode = error,
                ErrorMessage = LookupErrorMessage(error)
            };

            // if this is a client error, we need to build up the 
            // response back to the client, and provide it in the 
            // error view model so the UI can build the link/form
            if (errorType == ErrorTypes.Client)
            {
                errorModel.ReturnInfo = new ClientReturnInfo
                {
                    ClientId = request.ClientId,
                    ClientName = request.Client.ClientName,
                };

                var response = new AuthorizeResponse
                {
                    Request = request,
                    IsError = true,
                    Error = error,
                    State = request.State,
                    RedirectUri = request.RedirectUri
                };

                if (request.ResponseMode == Constants.ResponseModes.FormPost)
                {
                    errorModel.ReturnInfo.Uri = request.RedirectUri;
                    errorModel.ReturnInfo.PostBody = AuthorizeFormPostResult.BuildFormBody(response, _encoder);
                }
                else
                {
                    errorModel.ReturnInfo.Uri = request.RedirectUri = AuthorizeRedirectResult.BuildUri(response);
                }
            }

            return new ErrorPageResult(errorModel);
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