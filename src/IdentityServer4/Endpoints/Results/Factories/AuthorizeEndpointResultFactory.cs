// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    class AuthorizeEndpointResultFactory : IAuthorizeEndpointResultFactory
    {
        private readonly ILogger<AuthorizeEndpointResultFactory> _logger;
        private readonly IdentityServerContext _context;
        private readonly IAuthorizeResponseGenerator _responseGenerator;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageStore<ErrorMessage> _errorMessageStore;
        private readonly ClientListCookie _clientListCookie;

        public AuthorizeEndpointResultFactory(
            ILogger<AuthorizeEndpointResultFactory> logger,
            IdentityServerContext context,
            IAuthorizeResponseGenerator responseGenerator,
            ILocalizationService localizationService,
            IMessageStore<ErrorMessage> errorMessageStore,
            ClientListCookie clientListCookie)
        {
            _logger = logger;
            _context = context;
            _responseGenerator = responseGenerator;
            _localizationService = localizationService;
            _errorMessageStore = errorMessageStore;
            _clientListCookie = clientListCookie;
        }

        public Task<IEndpointResult> CreateLoginResultAsync(ValidatedAuthorizeRequest request)
        {
            var url = _context.HttpContext.Request.PathBase.ToString().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.AuthorizeAfterLogin;
            url = url.AddQueryString(request.Raw.ToQueryString());

            var result = new LoginPageResult(_context.Options.UserInteractionOptions, url);
            return Task.FromResult<IEndpointResult>(result);
        }

        public Task<IEndpointResult> CreateConsentResultAsync(ValidatedAuthorizeRequest request)
        {
            var url = _context.HttpContext.Request.PathBase.ToString().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.AuthorizeAfterConsent;
            url = url.AddQueryString(request.Raw.ToQueryString());

            var result = new ConsentPageResult(_context.Options.UserInteractionOptions, url);
            return Task.FromResult<IEndpointResult>(result);
        }

        public async Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            if (errorType == ErrorTypes.Client && request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request must be passed when error type is Client.");
            }

            AuthorizeResponse response = null;

            if (errorType == ErrorTypes.Client)
            {
                response = new AuthorizeResponse
                {
                    Request = request,
                    IsError = true,
                    Error = error,
                    State = request.State,
                    RedirectUri = request.RedirectUri
                };

                // do some early checks to see if we will end up not generating an error page
                if (error == OidcConstants.AuthorizeErrors.AccessDenied)
                {
                    return await CreateAuthorizeResultAsync(response);
                }

                if (request.PromptMode == OidcConstants.PromptModes.None &&
                    request.Client.AllowPromptNone == true &&
                    (error == OidcConstants.AuthorizeErrors.LoginRequired ||
                     error == OidcConstants.AuthorizeErrors.ConsentRequired ||
                     error == OidcConstants.AuthorizeErrors.InteractionRequired)
                )
                {
                    // todo: verify these are the right conditions to allow
                    // redirecting back to client
                    // https://tools.ietf.org/html/draft-bradley-oauth-open-redirector-00
                    return await CreateAuthorizeResultAsync(response);
                }
                else
                {
                    //_logger.LogWarning("Rendering error page due to prompt=none, client does not allow prompt mode none, response is query, and ");
                }
            }

            // we now know we must show error page
            var msg = _localizationService.GetMessage(error);
            if (msg.IsMissing())
            {
                msg = error;
            }

            var errorModel = new ErrorMessage
            {
                RequestId = _context.HttpContext.TraceIdentifier,
                ErrorCode = error,
                ErrorDescription = msg
            };

            if (errorType == ErrorTypes.Client)
            {
                // if this is a client error, we need to build up the 
                // response back to the client, and provide it in the 
                // error view model so the UI can build the link/form
                errorModel.ReturnInfo = new ClientReturnInfo
                {
                    ClientId = request.ClientId,
                };

                if (request.ResponseMode == OidcConstants.ResponseModes.Query ||
                    request.ResponseMode == OidcConstants.ResponseModes.Fragment)
                {
                    errorModel.ReturnInfo.Uri = request.RedirectUri = AuthorizeRedirectResult.BuildUri(response);
                }
                else if (request.ResponseMode == OidcConstants.ResponseModes.FormPost)
                {
                    errorModel.ReturnInfo.Uri = request.RedirectUri;
                    errorModel.ReturnInfo.PostBody = AuthorizeFormPostResult.BuildFormBody(response);
                }
                else
                {
                    _logger.LogError("Unsupported response mode.");
                    throw new InvalidOperationException("Unsupported response mode");
                }
            }

            var message = new MessageWithId<ErrorMessage>(errorModel);
            await _errorMessageStore.WriteAsync(message.Id, message);

            return new ErrorPageResult(_context.Options.UserInteractionOptions, message.Id);
        }

        public async Task<IEndpointResult> CreateAuthorizeResultAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateResponseAsync(request);
            return await CreateAuthorizeResultAsync(response);
        }

        Task<IEndpointResult> CreateAuthorizeResultAsync(AuthorizeResponse response)
        {
            var request = response.Request;

            IEndpointResult result = null;

            if (request.ResponseMode == OidcConstants.ResponseModes.Query ||
                request.ResponseMode == OidcConstants.ResponseModes.Fragment)
            {
                result = new AuthorizeRedirectResult(response);
            }
            if (request.ResponseMode == OidcConstants.ResponseModes.FormPost)
            {
                result = new AuthorizeFormPostResult(response);
            }

            if (result != null)
            {
                if (response.IsError == false)
                {
                    _logger.LogDebug("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                    _clientListCookie.AddClient(request.ClientId);
                }

                return Task.FromResult(result);
            }

            _logger.LogError("Unsupported response mode.");
            throw new InvalidOperationException("Unsupported response mode");
        }
    }
}
