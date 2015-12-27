using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Results
{
    public abstract class AuthorizeResult : IResult
    {
        public AuthorizeResult()
        {
        }

        public abstract Task ExecuteAsync(HttpContext context, ILogger logger);
    }
}
