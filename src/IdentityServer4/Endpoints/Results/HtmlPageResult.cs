// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Hosting;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNet.Http;

namespace IdentityServer4.Endpoints.Results
{
    abstract class HtmlPageResult : IEndpointResult
    {
        public bool DisableCache { get; set; } = true;

        protected abstract string GetHtml();

        public async Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.ContentType = "text/html; charset=UTF-8";
            if (DisableCache)
            {
                context.HttpContext.Response.SetNoCache();
            }

            var html = GetHtml();
            await context.HttpContext.Response.WriteAsync(html, Encoding.UTF8);
        }
    }
}
