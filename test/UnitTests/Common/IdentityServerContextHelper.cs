using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests.Common
{
    public class IdentityServerContextHelper
    {
        public static IdentityServerContext Create(HttpContext context = null, IdentityServerOptions options = null)
        {
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = context ?? new DefaultHttpContext();
            return new IdentityServerContext(accessor, options ?? new IdentityServerOptions());
        }
    }
}
