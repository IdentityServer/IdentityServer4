// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http;
using System.Text;

namespace IdentityServer4.Core.Endpoints.Results
{
    class AuthorizeFormPostResult : AuthorizeResult
    {
        const string _html = "<!DOCTYPE html><html><head><title>Submit this form</title><meta name='viewport' content='width=device-width, initial-scale=1.0' /></head><body><form method='post' action='{uri}'>{body}</form><script>(function(){document.forms[0].submit();})();</script></body></html>";

        public AuthorizeFormPostResult(AuthorizeResponse response)
            : base(response)
        {
        }

        internal static string BuildFormBody(AuthorizeResponse response)
        {
            return response.ToNameValueCollection().ToFormPost();
        }

        public override async Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.ContentType = "text/html; charset=UTF-8";
            context.HttpContext.Response.SetNoCache();

            var html = _html;
            html = html.Replace("{uri}", Response.RedirectUri);
            html = html.Replace("{body}", BuildFormBody(Response));

            await context.HttpContext.Response.WriteAsync(html, Encoding.UTF8);
        }
    }
}
