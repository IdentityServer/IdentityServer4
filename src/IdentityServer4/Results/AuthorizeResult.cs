using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    abstract class AuthorizeResult : IEndpointResult
    {
        public AuthorizeResponse Response { get; private set; }

        public AuthorizeResult(AuthorizeResponse response)
        {
            Response = response;
        }

        public abstract Task ExecuteAsync(IdentityServerContext context);
    }
}
