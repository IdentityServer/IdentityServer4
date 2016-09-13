// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Services;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Stores;
using IdentityServer4.Models;
using System.Net;
using System;

namespace IdentityServer4.Endpoints.Results
{
    class EndSessionCallbackResult : IEndpointResult
    {
        private readonly EndSessionCallbackValidationResult _result;

        public EndSessionCallbackResult(EndSessionCallbackValidationResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            _result = result;
        }

        internal EndSessionCallbackResult(
            EndSessionCallbackValidationResult result,
            ISessionIdService sessionId,
            IClientSessionService clientList,
            IMessageStore<LogoutMessage> logoutMessageStore)
            : this(result)
        {
            _sessionId = sessionId;
            _clientList = clientList;
            _logoutMessageStore = logoutMessageStore;
        }

        private ISessionIdService _sessionId;
        private IClientSessionService _clientList;
        private IMessageStore<LogoutMessage> _logoutMessageStore;

        void Init(HttpContext context)
        {
            _sessionId = _sessionId ?? context.RequestServices.GetRequiredService<ISessionIdService>();
            _clientList = _clientList ?? context.RequestServices.GetRequiredService<IClientSessionService>();
            _logoutMessageStore = _logoutMessageStore ?? context.RequestServices.GetRequiredService<IMessageStore<LogoutMessage>>();
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            if (_result.LogoutId != null)
            {
                var logoutMessageStore = context.RequestServices.GetRequiredService<IMessageStore<LogoutMessage>>();
                await logoutMessageStore.DeleteAsync(_result.LogoutId);
            }

            if (_result.IsError)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                _sessionId.RemoveCookie();
                _clientList.RemoveCookie();

                var html = GetHtml();

                context.Response.SetNoCache();
                await context.Response.WriteHtmlAsync(html);
            }
        }

        string GetHtml()
        {
            string framesHtml = null;

            if (_result.ClientLogoutUrls != null && _result.ClientLogoutUrls.Any())
            {
                var frameUrls = _result.ClientLogoutUrls.Select(url => $"<iframe style='display:none' width='0' height='0' src='{url}'></iframe>");
                framesHtml = frameUrls.Aggregate((x, y) => x + y);
            }

            return $"<!DOCTYPE html><html><body>{framesHtml}</body></html>";
        }
    }
}
