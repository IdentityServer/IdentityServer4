// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http;

namespace IdentityServer4.Core.Endpoints.Results
{
    class AuthorizeFormPostResult : AuthorizeResult
    {
        public AuthorizeFormPostResult(AuthorizeResponse response)
            : base(response)
        {
        }

        internal static string BuildFormBody(AuthorizeResponse response)
        {
            return response.ToNameValueCollection().ToFormPost();
        }

        public override Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.SetNoCache();

            // todo: render response <form>

            return Task.FromResult(0);
        }
    }
}
