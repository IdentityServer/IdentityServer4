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
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    class AuthorizeEndpointResultGenerator : IAuthorizeEndpointResultGenerator
    {
        private readonly ILogger<AuthorizeEndpointResultGenerator> _logger;
        private readonly IdentityServerContext _context;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageStore<SignInMessage> _signInMessageStore;
        private readonly ClientListCookie _clientListCookie;
        private readonly IMessageStore<UserConsentRequestMessage> _consentRequestStore;

        public AuthorizeEndpointResultGenerator(
            ILogger<AuthorizeEndpointResultGenerator> logger,
            IdentityServerContext context,
            ILocalizationService localizationService,
            IMessageStore<SignInMessage> signInMessageStore,
            IMessageStore<UserConsentRequestMessage> consentRequestStore,
            ClientListCookie clientListCookie)
        {
            _logger = logger;
            _context = context;
            _localizationService = localizationService;
            _signInMessageStore = signInMessageStore;
            _consentRequestStore = consentRequestStore;
            _clientListCookie = clientListCookie;
        }

        public async Task<IEndpointResult> CreateLoginResultAsync(SignInMessage message)
        {
            var id = await _signInMessageStore.WriteAsync(message);
            return new LoginPageResult(id);
        }

        public async Task<IEndpointResult> CreateConsentResultAsync(ValidatedAuthorizeRequest validatedRequest, NameValueCollection parameters)
        {
            var message = new UserConsentRequestMessage(validatedRequest, parameters);
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

            return new ErrorPageResult(errorModel);
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
