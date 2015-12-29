using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    interface IEndpointRouter
    {
        // TODO: does this need to be async?
        IEndpoint Find(HttpContext context);
    }
}
