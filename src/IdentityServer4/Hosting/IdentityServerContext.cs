using System;
using IdentityServer4.Core.Configuration;
using Microsoft.AspNet.Http;

namespace IdentityServer4.Core.Hosting
{
    public class IdentityServerContext
    {
        public HttpContext HttpContext { get; internal set; }
        public IdentityServerOptions Options { get; set; }

        public IdentityServerContext(IHttpContextAccessor contextAccessor, IdentityServerOptions options)
        {
            HttpContext = contextAccessor.HttpContext;
            Options = options;
        }
    }
}