using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.ViewModels;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    public class ConsentPageResult : IEndpointResult
    {
        public ConsentPageResult()
        {
        }

        public Task ExecuteAsync(IdentityServerContext context)
        {
            return Task.FromResult(0);
        }
    }
}
