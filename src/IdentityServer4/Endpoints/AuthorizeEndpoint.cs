using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Results;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Extensions;

namespace IdentityServer4.Core.Endpoints
{
    public class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;

        public AuthorizeEndpoint(IEventService events, ILogger<AuthorizeEndpoint> logger)
        {
            _events = events;
            _logger = logger;
        }

        public async Task<IResult> ProcessAsync(HttpContext context)
        {
            return new AuthorizeResult();
            //throw new InvalidOperationException("Invalid authorize endpoint outcome");
        }

        //private async Task RaiseSuccessEventAsync(string token, string tokenStatus, string scopeName)
        //{
        //    await _events.RaiseSuccessfulIntrospectionEndpointEventAsync(
        //        token,
        //        tokenStatus,
        //        scopeName);
        //}

        //private async Task RaiseFailureEventAsync(string error, string token, string scopeName)
        //{
        //    await _events.RaiseFailureIntrospectionEndpointEventAsync(
        //        error, token, scopeName);
        //}
    }
}