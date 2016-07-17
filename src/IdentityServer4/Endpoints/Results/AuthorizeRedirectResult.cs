// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNet.Http;
using IdentityModel;

namespace IdentityServer4.Endpoints.Results
{
    class AuthorizeRedirectResult : IEndpointResult
    {
        public AuthorizeResponse Response { get; private set; }

        public AuthorizeRedirectResult(AuthorizeResponse response)
        {
            Response = response;
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

        public Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.SetNoCache();
            context.HttpContext.Response.Redirect(BuildUri(Response));
            return Task.FromResult(0);
        }
    }
}
