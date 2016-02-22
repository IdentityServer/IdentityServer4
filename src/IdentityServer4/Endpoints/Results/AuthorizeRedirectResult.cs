// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http;
using IdentityModel;

namespace IdentityServer4.Core.Endpoints.Results
{
    class AuthorizeRedirectResult : AuthorizeResult
    {
        public AuthorizeRedirectResult(AuthorizeResponse response)
            : base(response)
        {
        }

        internal static string BuildUri(AuthorizeResponse response)
        {
            var uri = response.RedirectUri;
            var query = response.ToNameValueCollection().ToQueryString();

            if (response.Request.ResponseMode == OidcConstants.ResponseModes.Query)
            {
                uri = uri.AddQueryString(query);
            }
            else
            {
                uri = uri.AddHashFragment(query);
            }

            if (response.IsError && !uri.Contains("#"))
            {
                // https://tools.ietf.org/html/draft-bradley-oauth-open-redirector-00
                uri += "#_=_";
            }

            return uri;
        }

        public override Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.SetNoCache();
            context.HttpContext.Response.Redirect(BuildUri(Response));
            return Task.FromResult(0);
        }
    }
}
