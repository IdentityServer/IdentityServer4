using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    class AuthorizationResultGenerator : IAuthorizationResultGenerator
    {
        private readonly ILogger<AuthorizationResultGenerator> _logger;
        private readonly IdentityServerContext _context;
        private readonly ILocalizationService _localizationService;
        private readonly IHtmlEncoder _htmlEncoder;
        private readonly IUrlEncoder _urlEncoder;
        private readonly IMessageStore<SignInMessage> _signInMessageStore;
        private readonly ClientListCookie _clientListCookie;

        public AuthorizationResultGenerator(
            ILogger<AuthorizationResultGenerator> logger,
            IdentityServerContext context,
            ILocalizationService localizationService,
            IHtmlEncoder htmlEncoder,
            IUrlEncoder urlEncoder,
            IMessageStore<SignInMessage> signInMessageStore,
            ClientListCookie clientListCookie)
        {
            _logger = logger;
            _context = context;
            _localizationService = localizationService;
            _htmlEncoder = htmlEncoder;
            _urlEncoder = urlEncoder;
            _signInMessageStore = signInMessageStore;
            _clientListCookie = clientListCookie;
        }

        public Task<IEndpointResult> CreateConsentResultAsync()
        {
            return Task.FromResult<IEndpointResult>(new ConsentPageResult());

            //string loginWithDifferentAccountUrl = null;
            //if (validatedRequest.HasIdpAcrValue() == false)
            //{
            //    loginWithDifferentAccountUrl = Url.Route(Constants.RouteNames.Oidc.SwitchUser, null)
            //        .AddQueryString(requestParameters.ToQueryString());
            //}

            //var env = Request.GetOwinEnvironment();
            //var consentModel = new ConsentViewModel
            //{
            //    RequestId = env.GetRequestId(),
            //    SiteName = _options.SiteName,
            //    SiteUrl = env.GetIdentityServerBaseUrl(),
            //    ErrorMessage = errorMessage,
            //    CurrentUser = env.GetCurrentUserDisplayName(),
            //    LogoutUrl = env.GetIdentityServerLogoutUrl(),
            //    ClientName = validatedRequest.Client.ClientName,
            //    ClientUrl = validatedRequest.Client.ClientUri,
            //    ClientLogoUrl = validatedRequest.Client.LogoUri,
            //    IdentityScopes = validatedRequest.GetIdentityScopes(this._localizationService),
            //    ResourceScopes = validatedRequest.GetResourceScopes(this._localizationService),
            //    AllowRememberConsent = validatedRequest.Client.AllowRememberConsent,
            //    RememberConsent = consent == null || consent.RememberConsent,
            //    LoginWithDifferentAccountUrl = loginWithDifferentAccountUrl,
            //    ConsentUrl = Url.Route(Constants.RouteNames.Oidc.Consent, null).AddQueryString(requestParameters.ToQueryString()),
            //    AntiForgery = _antiForgeryToken.GetAntiForgeryToken()
            //};

            //return new ConsentActionResult(_viewService, consentModel, validatedRequest);
        }

        public async Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            var msg = _localizationService.GetMessage(error);
            if (msg.IsMissing())
            {
                msg = error;
            }

            var errorModel = new ErrorViewModel
            {
                RequestId = _context.GetRequestId(),
                ErrorCode = error,
                ErrorMessage = msg
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

                if (request.PromptMode == Constants.PromptModes.None)
                {
                    return await CreateAuthorizeResultAsync(response);
                }

                errorModel.ReturnInfo = new ClientReturnInfo
                {
                    ClientId = request.ClientId,
                    ClientName = request.Client.ClientName,
                };

                if (request.ResponseMode == Constants.ResponseModes.Query ||
                    request.ResponseMode == Constants.ResponseModes.Fragment)
                {
                    errorModel.ReturnInfo.Uri = request.RedirectUri = AuthorizeRedirectResult.BuildUri(response, _urlEncoder);
                }
                else if (request.ResponseMode == Constants.ResponseModes.FormPost)
                {
                    errorModel.ReturnInfo.Uri = request.RedirectUri;
                    errorModel.ReturnInfo.PostBody = AuthorizeFormPostResult.BuildFormBody(response, _htmlEncoder);
                }
                else
                {
                    _logger.LogError("Unsupported response mode.");
                    throw new InvalidOperationException("Unsupported response mode");
                }
            }

            return new ErrorPageResult(errorModel);
        }

        public async Task<IEndpointResult> CreateLoginResultAsync(SignInMessage message)
        {
            var id = await _signInMessageStore.WriteAsync(message);

            var url = _context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Constants.RoutePaths.Login;
            url += url.AddQueryString("id=" + id);

            return new LoginPageResult(url);
        }

        public Task<IEndpointResult> CreateAuthorizeResultAsync(AuthorizeResponse response)
        {
            var request = response.Request;

            if (request.ResponseMode == Constants.ResponseModes.Query ||
                request.ResponseMode == Constants.ResponseModes.Fragment)
            {
                _logger.LogDebug("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                _clientListCookie.AddClient(request.ClientId);

                return Task.FromResult<IEndpointResult>(new AuthorizeRedirectResult(response, _urlEncoder));
            }

            if (request.ResponseMode == Constants.ResponseModes.FormPost)
            {
                _logger.LogDebug("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                _clientListCookie.AddClient(request.ClientId);

                return Task.FromResult<IEndpointResult>(new AuthorizeFormPostResult(response, _htmlEncoder));
            }

            _logger.LogError("Unsupported response mode.");
            throw new InvalidOperationException("Unsupported response mode");
        }
    }
}
