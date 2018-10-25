// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer4.Services
{
    internal class DefaultIdentityServerInteractionService : IIdentityServerInteractionService
    {
        private readonly ISystemClock _clock;
        private readonly IHttpContextAccessor _context;
        private readonly IMessageStore<LogoutMessage> _logoutMessageStore;
        private readonly IMessageStore<ErrorMessage> _errorMessageStore;
        private readonly IConsentMessageStore _consentMessageStore;
        private readonly IPersistedGrantService _grants;
        private readonly IUserSession _userSession;
        private readonly ILogger _logger;
        private readonly ReturnUrlParser _returnUrlParser;

        public DefaultIdentityServerInteractionService(
            ISystemClock clock,
            IHttpContextAccessor context,
            IMessageStore<LogoutMessage> logoutMessageStore,
            IMessageStore<ErrorMessage> errorMessageStore,
            IConsentMessageStore consentMessageStore,
            IPersistedGrantService grants,
            IUserSession userSession,
            ReturnUrlParser returnUrlParser,
            ILogger<DefaultIdentityServerInteractionService> logger)
        {
            _clock = clock;
            _context = context;
            _logoutMessageStore = logoutMessageStore;
            _errorMessageStore = errorMessageStore;
            _consentMessageStore = consentMessageStore;
            _grants = grants;
            _userSession = userSession;
            _returnUrlParser = returnUrlParser;
            _logger = logger;
        }

        public async Task<AuthorizationRequest> GetAuthorizationContextAsync(string returnUrl)
        {
            var result = await _returnUrlParser.ParseAsync(returnUrl);

            if (result != null)
            {
                _logger.LogTrace("AuthorizationRequest being returned");
            }
            else
            {
                _logger.LogTrace("No AuthorizationRequest being returned");
            }

            return result;
        }

        public async Task<LogoutRequest> GetLogoutContextAsync(string logoutId)
        {
            var msg = await _logoutMessageStore.ReadAsync(logoutId);
            var iframeUrl = await _context.HttpContext.GetIdentityServerSignoutFrameCallbackUrlAsync(msg?.Data);
            return new LogoutRequest(iframeUrl, msg?.Data);
        }

        public async Task<string> CreateLogoutContextAsync()
        {
            var user = await _userSession.GetUserAsync();
            if (user != null)
            {
                var clientIds = await _userSession.GetClientListAsync();
                if (clientIds.Any())
                {
                    var sid = await _userSession.GetSessionIdAsync();
                    var msg = new Message<LogoutMessage>(new LogoutMessage
                    {
                        SubjectId = user?.GetSubjectId(),
                        SessionId = sid,
                        ClientIds = clientIds
                    }, _clock.UtcNow.UtcDateTime);
                    var id = await _logoutMessageStore.WriteAsync(msg);
                    return id;
                }
            }

            return null;
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
                var user = await _userSession.GetUserAsync();
                subject = user?.GetSubjectId();
            }

            if (subject == null && consent.Granted)
            {
                throw new ArgumentNullException(nameof(subject), "User is not currently authenticated, and no subject id passed");
            }

            var consentRequest = new ConsentRequest(request, subject);
            await _consentMessageStore.WriteAsync(consentRequest.Id, new Message<ConsentResponse>(consent, _clock.UtcNow.UtcDateTime));
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            var result = _returnUrlParser.IsValidReturnUrl(returnUrl);

            if (result)
            {
                _logger.LogTrace("IsValidReturnUrl true");
            }
            else
            {
                _logger.LogTrace("IsValidReturnUrl false");
            }

            return result;
        }

        public async Task<IEnumerable<Consent>> GetAllUserConsentsAsync()
        {
            var user = await _userSession.GetUserAsync();
            if (user != null)
            {
                var subject = user.GetSubjectId();
                return await _grants.GetAllGrantsAsync(subject);
            }

            return Enumerable.Empty<Consent>();
        }

        public async Task RevokeUserConsentAsync(string clientId)
        {
            var user = await _userSession.GetUserAsync();
            if (user != null)
            {
                var subject = user.GetSubjectId();
                await _grants.RemoveAllGrantsAsync(subject, clientId);
            }
        }

        public async Task RevokeTokensForCurrentSessionAsync()
        {
            var user = await _userSession.GetUserAsync();
            if (user != null)
            {
                var subject = user.GetSubjectId();
                var clients = await _userSession.GetClientListAsync();
                foreach (var client in clients)
                {
                    await _grants.RemoveAllGrantsAsync(subject, client);
                }
            }
        }
    }
}
