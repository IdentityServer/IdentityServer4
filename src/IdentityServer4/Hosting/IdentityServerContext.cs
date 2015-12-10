using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public class IdentityServerContext
    {
        public HttpContext HttpContext { get; internal set; }

        public IdentityServerContext(IHttpContextAccessor contextAccessor)
        {
            HttpContext = contextAccessor.HttpContext;
        }
    }
}