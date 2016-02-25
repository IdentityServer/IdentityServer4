using IdentityServer4.Core.Endpoints.Results;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.ResponseHandling;
using IdentityServer4.Core.Validation;
using Microsoft.AspNet.Http;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Endpoints
{
    class EndSessionEndpoint : IEndpoint
    {
        private readonly ILogger<EndSessionEndpoint> _logger;
        private readonly IEndSessionRequestValidator _validator;
        private readonly IEndSessionResponseGenerator _generator;

        public EndSessionEndpoint(
            ILogger<EndSessionEndpoint> logger,
            IEndSessionRequestValidator validator, 
            IEndSessionResponseGenerator generator)
        {
            _logger = logger;
            _validator = validator;
            _generator = generator;
        }

        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            _logger.LogInformation("Start end session request");

            if (context.HttpContext.Request.Method != "GET" && context.HttpContext.Request.Method != "POST")
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            NameValueCollection parameters = null;

            if (context.HttpContext.Request.Method == "GET")
            {
                parameters = context.HttpContext.Request.Query.AsNameValueCollection();
            }

            else if (context.HttpContext.Request.Method == "POST")
            {
                if (context.HttpContext.Request.HasFormContentType && context.HttpContext.Request.Body.Length >= 0)
                {
                    parameters = context.HttpContext.Request.Form.AsNameValueCollection();
                }
            }

            var result = await _validator.ValidateAsync(parameters, context.HttpContext.User);

            if (result.IsError)
            {
                await context.HttpContext.Response.WriteAsync(result.Error);

                return new StatusCodeResult(HttpStatusCode.BadRequest);
            }

            var message = await _generator.ProcessAsync(result);

            _logger.LogInformation("End end session request");

            return new LogoutResult(message.Id);
        }
    }
}
