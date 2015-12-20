using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Results
{
    public class AuthorizeResult : IResult
    {
        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            context.Response.StatusCode = 200;
            return Task.FromResult(0);
        }
    }
}
