// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    class AuthorizeFormPostResult : IEndpointResult
    {
        public AuthorizeResponse Response { get; private set; }

        public AuthorizeFormPostResult(AuthorizeResponse response)
        {
            Response = response;
        }

        const string _html = "<!DOCTYPE html><html><head><title>Submit this form</title><meta name='viewport' content='width=device-width, initial-scale=1.0' /></head><body><form method='post' action='{uri}'>{body}</form><script>(function(){document.forms[0].submit();})();</script></body></html>";

        string GetHtml()
        {
            var html = _html;

            html = html.Replace("{uri}", Response.RedirectUri);
            html = html.Replace("{body}", BuildFormBody(Response));

            return html;
        }

        string BuildFormBody(AuthorizeResponse response)
        {
            return response.ToNameValueCollection().ToFormPost();
        }

        public Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();
            return context.Response.WriteHtmlAsync(GetHtml());
        }
    }
}
