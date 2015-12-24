using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Results;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net;
using System.Security.Claims;
using IdentityServer4.Core.Configuration;

namespace IdentityServer4.Core.Endpoints
{
    class AuthorizeEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;

        public AuthorizeEndpoint(
            IEventService events, 
            ILogger<AuthorizeEndpoint> logger,
            IdentityServerOptions options)
        {
            _events = events;
            _logger = logger;
            _options = options;
        }

        public async Task<IResult> ProcessAsync(HttpContext context)
        {
            if (context.Request.Method != "GET")
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            _logger.LogInformation("Start Authorize Request");

            var values = context.Request.Query.AsNameValueCollection();
            var user = await context.Authentication.AuthenticateAsync(_options.AuthenticationOptions.EffectivePrimaryAuthenticationScheme);
            var result = await ProcessRequestAsync(values, user);

            _logger.LogInformation("End Authorize Request");

            return result;
        }

        public Task<IResult> ProcessRequestAsync(NameValueCollection parameters, ClaimsPrincipal user)
        {
            return Task.FromResult<IResult>(null);
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