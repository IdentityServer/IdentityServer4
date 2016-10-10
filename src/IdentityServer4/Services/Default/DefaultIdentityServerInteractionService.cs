// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IdentityServer4.Services.Default
{
    class DefaultIdentityServerInteractionService : IIdentityServerInteractionService
    {
        private readonly IdentityServerOptions _options;
        private readonly IHttpContextAccessor _context;
        private readonly IAuthorizeRequestValidator _validator;
        private readonly IMessageStore<LogoutMessage> _logoutMessageStore;
        private readonly IMessageStore<ErrorMessage> _errorMessageStore;
        private readonly IMessageStore<ConsentResponse> _consentMessageStore;
        private readonly IPersistedGrantService _grants;
        private readonly IClientSessionService _clientSessionService;
        private readonly ILogger<DefaultIdentityServerInteractionService> _logger;

        public DefaultIdentityServerInteractionService(
            IdentityServerOptions options,
            IHttpContextAccessor context,
            IAuthorizeRequestValidator validator,
            IMessageStore<LogoutMessage> logoutMessageStore,
            IMessageStore<ErrorMessage> errorMessageStore,
            IMessageStore<ConsentResponse> consentMessageStore,
            IPersistedGrantService grants, 
            IClientSessionService clientSessionService,
            ILogger<DefaultIdentityServerInteractionService> logger)
        {
            _options = options;
            _context = context;
            _validator = validator;
            _logoutMessageStore = logoutMessageStore;
            _errorMessageStore = errorMessageStore;
            _consentMessageStore = consentMessageStore;
            _grants = grants;
            _clientSessionService = clientSessionService;
            _logger = logger;
        }

        public async Task<AuthorizationRequest> GetAuthorizationContextAsync(string returnUrl)
        {
            if (returnUrl != null && IsValidReturnUrl(returnUrl))
            {
                var parameters = returnUrl.ReadQueryStringAsNameValueCollection();
                var user = await _context.HttpContext.GetIdentityServerUserAsync();
                var result = await _validator.ValidateAsync(parameters, user);
                if (!result.IsError)
                {
                    _logger.LogTrace("AuthorizationRequest being returned");
                    return new AuthorizationRequest(result.ValidatedRequest);
                }
            }

            _logger.LogTrace("No AuthorizationRequest being returned");
            return null;
        }

        public async Task<LogoutRequest> GetLogoutContextAsync(string logoutId)
        {
            var iframeUrl = await _context.HttpContext.GetIdentityServerSignoutFrameCallbackUrlAsync();
            var msg = await _logoutMessageStore.ReadAsync(logoutId);

            if (iframeUrl != null && logoutId != null && msg != null)
            {
                iframeUrl = iframeUrl.AddQueryString(_options.UserInteractionOptions.LogoutIdParameter, logoutId);
            }

            return new LogoutRequest(iframeUrl, msg?.Data);
        }

        public async Task<ErrorMessage> GetErrorContextAsync(string errorId)
        {
            if (errorId != null)
            { 
                var result = await _errorMessageStore.ReadAsync(errorId);
                var data = result?.Data;
                if (data != null)
                {
                    _logger.LogTrace("Error context loaded");
                }
                else
                {
                    _logger.LogTrace("No error context found");
                }
                return data;
            }

            _logger.LogTrace("No error context found");

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

                if (returnUrl.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
                    returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeAfterLogin, StringComparison.Ordinal) || 
                    returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeAfterConsent, StringComparison.Ordinal))
                {
                    _logger.LogTrace("returnUrl is valid");
                    return true;
                }
            }

            _logger.LogTrace("returnUrl is not valid");
            return false;
        }

        public async Task<IEnumerable<Consent>> GetAllUserConsentsAsync()
        {
            var user = await _context.HttpContext.GetIdentityServerUserAsync();
            if (user != null)
            {
                var subject = user.GetSubjectId();
                return await _grants.GetAllGrantsAsync(subject);
            }

            return Enumerable.Empty<Consent>();
        }

        public async Task RevokeUserConsentAsync(string clientId)
        {
            var user = await _context.HttpContext.GetIdentityServerUserAsync();
            if (user != null)
            {
                var subject = user.GetSubjectId();
                await _grants.RemoveAllGrantsAsync(subject, clientId);
            }
        }

        public async Task RevokeTokensForCurrentSessionAsync()
        {
            var user = await _context.HttpContext.GetIdentityServerUserAsync();
            if (user != null)
            {
                var subject = user.GetSubjectId();
                var clients = await _clientSessionService.GetClientListAsync();
                foreach (var client in clients)
                {
                    await _grants.RemoveAllGrantsAsync(subject, client);
                }
            }
        }
    }
}
