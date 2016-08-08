using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNet.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Services.Default
{
    class DefaultUserInteractionService : IUserInteractionService
    {
        private readonly IdentityServerContext _context;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IMessageStore<LogoutMessage> _logoutMessageStore;
        private readonly IMessageStore<ErrorMessage> _errorMessageStore;
        private readonly IMessageStore<ConsentResponse> _consentMessageStore;

        public DefaultUserInteractionService(
            IdentityServerContext context,
            IAuthorizeRequestValidator validator,
            IMessageStore<LogoutMessage> logoutMessageStore,
            IMessageStore<ErrorMessage> errorMessageStore,
            IMessageStore<ConsentResponse> consentMessageStore)
        {
            _context = context;
            _validator = validator;
            _logoutMessageStore = logoutMessageStore;
            _errorMessageStore = errorMessageStore;
            _consentMessageStore = consentMessageStore;
        }

        public Task<AuthorizationRequest> GetLoginContextAsync(string returnUrl = null)
        {
            return GetAuthorizeRequest(_context.Options.UserInteractionOptions.LoginReturnUrlParameter, returnUrl);
        }

        public async Task<LogoutRequest> GetLogoutContextAsync(string logoutId = null)
        {
            if (logoutId == null)
            {
                logoutId = _context.HttpContext.Request.Query[_context.Options.UserInteractionOptions.LogoutIdParameter].FirstOrDefault();
            }

            var iframeUrl = _context.GetIdentityServerSignoutFrameCallbackUrl();
            var msg = await _logoutMessageStore.ReadAsync(logoutId);

            if (logoutId != null && msg != null)
            {
                iframeUrl = iframeUrl.AddQueryString(_context.Options.UserInteractionOptions.LogoutIdParameter + "=" + logoutId);
            }
            return new LogoutRequest(iframeUrl, msg?.Data);
        }

        public Task<AuthorizationRequest> GetConsentContextAsync(string returnUrl = null)
        {
            return GetAuthorizeRequest(_context.Options.UserInteractionOptions.ConsentReturnUrlParameter, returnUrl);
        }

        async Task<AuthorizationRequest> GetAuthorizeRequest(string paramName, string paramValue)
        {
            if (paramValue == null)
            {
                paramValue = _context.HttpContext.Request.Query[paramName].FirstOrDefault();
            }

            if (paramValue != null && IsValidReturnUrl(paramValue))
            {
                var parameters = paramValue.ReadQueryStringAsNameValueCollection();
                var user = await _context.GetIdentityServerUserAsync();
                var result = await _validator.ValidateAsync(parameters, user);
                if (!result.IsError)
                {
                    return new AuthorizationRequest(result.ValidatedRequest);
                }
            }

            return null;
        }

        public async Task<ErrorMessage> GetErrorContextAsync(string errorId = null)
        {
            if (errorId == null)
            {
                StringValues values;
                if (_context.HttpContext.Request.Query.TryGetValue(_context.Options.UserInteractionOptions.ErrorIdParameter, out values))
                {
                    errorId = values.First();
                }
            }

            if (errorId != null)
            { 
                var result = await _errorMessageStore.ReadAsync(errorId);
                return result?.Data;
            }

            return null;
        }

        public async Task GrantConsentAsync(AuthorizationRequest request, ConsentResponse consent, string subject = null)
        {
            if (subject == null)
            {
                var user = await _context.GetIdentityServerUserAsync();
                subject = user.GetSubjectId();
            }

            var consentRequest = new ConsentRequest(request, subject);
            await _consentMessageStore.WriteAsync(consentRequest.Id, new Message<ConsentResponse>(consent));
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            if (returnUrl.IsLocalUrl())
            {
                var index = returnUrl.IndexOf('?');
                if (index >= 0)
                {
                    returnUrl = returnUrl.Substring(0, index);
                }

                if (returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeAfterLogin, StringComparison.Ordinal) || 
                    returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeAfterConsent, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
