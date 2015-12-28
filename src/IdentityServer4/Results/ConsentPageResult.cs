using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.ViewModels;
using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Results
{
    public class ConsentPageResult : IResult
    {
        public ConsentPageResult()
        {
        }

        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            return Task.FromResult(0);
        }
    }
}
