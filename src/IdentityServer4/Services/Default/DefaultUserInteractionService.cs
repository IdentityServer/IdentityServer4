// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Services.Default
{
    class DefaultUserInteractionService : IUserInteractionService
    {
        private readonly IdentityServerOptions _options;
        private readonly IHttpContextAccessor _context;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IMessageStore<LogoutMessage> _logoutMessageStore;
        private readonly IMessageStore<ErrorMessage> _errorMessageStore;
        private readonly IMessageStore<ConsentResponse> _consentMessageStore;

        public DefaultUserInteractionService(
            IdentityServerOptions options,
            IHttpContextAccessor context,
            IAuthorizeRequestValidator validator,
            IMessageStore<LogoutMessage> logoutMessageStore,
            IMessageStore<ErrorMessage> errorMessageStore,
            IMessageStore<ConsentResponse> consentMessageStore)
        {
            _options = options;
            _context = context;
            _validator = validator;
            _logoutMessageStore = logoutMessageStore;
            _errorMessageStore = errorMessageStore;
            _consentMessageStore = consentMessageStore;
        }

        public Task<AuthorizationRequest> GetLoginContextAsync(string returnUrl = null)
        {
            return GetAuthorizeRequest(_options.UserInteractionOptions.LoginReturnUrlParameter, returnUrl);
        }

        public async Task<LogoutRequest> GetLogoutContextAsync(string logoutId = null)
        {
            if (logoutId == null)
            {
                logoutId = _context.HttpContext.Request.Query[_options.UserInteractionOptions.LogoutIdParameter].FirstOrDefault();
            }

            var iframeUrl = _context.HttpContext.GetIdentityServerSignoutFrameCallbackUrl();
            if (iframeUrl != null)
            {
                var msg = await _logoutMessageStore.ReadAsync(logoutId);
                if (logoutId != null && msg != null)
                {
                    iframeUrl = iframeUrl.AddQueryString(_options.UserInteractionOptions.LogoutIdParameter + "=" + logoutId);
                }

                return new LogoutRequest(iframeUrl, msg?.Data);
            }

            return null;
        }

        public Task<AuthorizationRequest> GetConsentContextAsync(string returnUrl = null)
        {
            return GetAuthorizeRequest(_options.UserInteractionOptions.ConsentReturnUrlParameter, returnUrl);
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
                var user = await _context.HttpContext.GetIdentityServerUserAsync();
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
                if (_context.HttpContext.Request.Query.TryGetValue(_options.UserInteractionOptions.ErrorIdParameter, out values))
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
                var user = await _context.HttpContext.GetIdentityServerUserAsync();
                subject = user.GetSubjectId();
            }

            var consentRequest = new ConsentRequest(request, subject);
            await _consentMessageStore.WriteAsync(consentRequest.Id, new Message<ConsentResponse>(consent));
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            // TODO: allow remote urls, once supported
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
