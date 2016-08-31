// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Extensions;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using IdentityServer4.Services;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Stores;
using IdentityServer4.Models;
using System.Net;

namespace IdentityServer4.Endpoints.Results
{
    class EndSessionCallbackResult : IEndpointResult
    {
        private readonly EndSessionCallbackValidationResult _result;

        public EndSessionCallbackResult(EndSessionCallbackValidationResult result)
        {
            _result = result;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
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
                var sessionId = context.RequestServices.GetRequiredService<ISessionIdService>();
                sessionId.RemoveCookie();

                var clientList = context.RequestServices.GetRequiredService<IClientSessionService>();
                clientList.RemoveCookie();
                //clientList.Clear();

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
