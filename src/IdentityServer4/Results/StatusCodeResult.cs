using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Results
{
    public class StatusCodeResult : IResult
    {
        public int StatusCode { get; private set; }

        public StatusCodeResult(int statusCode)
        {
            StatusCode = statusCode;
        }

        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            context.Response.StatusCode = StatusCode;

            return Task.FromResult(0);
        }
    }
}