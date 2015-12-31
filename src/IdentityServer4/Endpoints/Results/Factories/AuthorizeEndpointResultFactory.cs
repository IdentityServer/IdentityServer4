// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints.Results
{
    class AuthorizeEndpointResultFactory : IAuthorizeEndpointResultFactory
    {
        private readonly ILogger<AuthorizeEndpointResultFactory> _logger;
        private readonly IdentityServerContext _context;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageStore<SignInMessage> _signInMessageStore;
        private readonly IMessageStore<UserConsentRequestMessage> _consentRequestStore;
        private readonly IMessageStore<ErrorMessage> _errorMessageStore;
        private readonly ClientListCookie _clientListCookie;

        public AuthorizeEndpointResultFactory(
            ILogger<AuthorizeEndpointResultFactory> logger,
            IdentityServerContext context,
            ILocalizationService localizationService,
            IMessageStore<SignInMessage> signInMessageStore,
            IMessageStore<UserConsentRequestMessage> consentRequestStore,
            IMessageStore<ErrorMessage> errorMessageStore,
            ClientListCookie clientListCookie)
        {
            _logger = logger;
            _context = context;
            _localizationService = localizationService;
            _signInMessageStore = signInMessageStore;
            _consentRequestStore = consentRequestStore;
            _errorMessageStore = errorMessageStore;
            _clientListCookie = clientListCookie;
        }

        public async Task<IEndpointResult> CreateLoginResultAsync(ValidatedAuthorizeRequest request)
        {
            var message = new SignInMessage();

            // build return URL to return to authorization
            var url = _context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Constants.RoutePaths.Oidc.Authorize;
            url.AddQueryString(request.Raw.ToQueryString());
            message.ReturnUrl = url;

            // let the login page know the client requesting authorization
            message.ClientId = request.ClientId;

            // pass through display mode to signin service
            if (request.DisplayMode.IsPresent())
            {
                message.DisplayMode = request.DisplayMode;
            }

            // pass through ui locales to signin service
            if (request.UiLocales.IsPresent())
            {
                message.UiLocales = request.UiLocales;
            }

            // pass through login_hint
            if (request.LoginHint.IsPresent())
            {
                message.LoginHint = request.LoginHint;
            }

            // look for well-known acr value -- idp
            var idp = request.GetIdP();
            if (idp.IsPresent())
            {
                message.IdP = idp;
            }

            // look for well-known acr value -- tenant
            var tenant = request.GetTenant();
            if (tenant.IsPresent())
            {
                message.Tenant = tenant;
            }

            // process acr values
            var acrValues = request.GetAcrValues();
            if (acrValues.Any())
            {
                message.AcrValues = acrValues;
            }

            var id = await _signInMessageStore.WriteAsync(message);

            return new LoginPageResult(id);
        }

        public async Task<IEndpointResult> CreateConsentResultAsync(ValidatedAuthorizeRequest request)
        {
            var message = new UserConsentRequestMessage()
            {
                ClientId = request.ClientId,
                ScopesRequested = request.RequestedScopes.ToArray(),
                AuthorizeRequestParameters = request.Raw
            };
            var id = await _consentRequestStore.WriteAsync(message);
            return new ConsentPageResult(id);
        }

        public async Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            if (errorType == ErrorTypes.Client && request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request must be passed when error type is Client.");
            }

            var msg = _localizationService.GetMessage(error);
            if (msg.IsMissing())
            {
                msg = error;
            }

            var errorModel = new ErrorMessage
            {
                RequestId = _context.GetRequestId(),
                ErrorCode = error,
                ErrorDescription = msg
            };

            // if this is a client error, we need to build up the 
            // response back to the client, and provide it in the 
            // error view model so the UI can build the link/form
            if (errorType == ErrorTypes.Client)
            {
                var response = new AuthorizeResponse
                {
                    Request = request,
                    IsError = true,
                    Error = error,
                    State = request.State,
                    RedirectUri = request.RedirectUri
                };

                if (error == Constants.AuthorizeErrors.AccessDenied)
                {
                    return await CreateAuthorizeResultAsync(response);
                }

                if (request.PromptMode == Constants.PromptModes.None && 
                    request.Client.AllowPromptNone == true &&
                    (error == Constants.AuthorizeErrors.LoginRequired || 
                     error == Constants.AuthorizeErrors.ConsentRequired || 
                     error == Constants.AuthorizeErrors.InteractionRequired)
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

                errorModel.ReturnInfo = new ClientReturnInfo
                {
                    ClientId = request.ClientId,
                    ClientName = request.Client.ClientName,
                };

                if (request.ResponseMode == Constants.ResponseModes.Query ||
                    request.ResponseMode == Constants.ResponseModes.Fragment)
                {
                    errorModel.ReturnInfo.Uri = request.RedirectUri = AuthorizeRedirectResult.BuildUri(response);
                }
                else if (request.ResponseMode == Constants.ResponseModes.FormPost)
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

            var id = await _errorMessageStore.WriteAsync(errorModel);
            return new ErrorPageResult(id);
        }

        public Task<IEndpointResult> CreateAuthorizeResultAsync(AuthorizeResponse response)
        {
            var request = response.Request;

            if (request.ResponseMode == Constants.ResponseModes.Query ||
                request.ResponseMode == Constants.ResponseModes.Fragment)
            {
                _logger.LogDebug("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                _clientListCookie.AddClient(request.ClientId);

                return Task.FromResult<IEndpointResult>(new AuthorizeRedirectResult(response));
            }

            if (request.ResponseMode == Constants.ResponseModes.FormPost)
            {
                _logger.LogDebug("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                _clientListCookie.AddClient(request.ClientId);

                return Task.FromResult<IEndpointResult>(new AuthorizeFormPostResult(response));
            }

            _logger.LogError("Unsupported response mode.");
            throw new InvalidOperationException("Unsupported response mode");
        }
    }
}
