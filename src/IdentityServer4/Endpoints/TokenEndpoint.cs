using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
{
    public class TokenEndpoint : IEndpoint
    {
        private readonly ILogger _logger;
        private readonly TokenRequestValidator _requestValidator;

        public TokenEndpoint(TokenRequestValidator requestValidator, ILoggerFactory loggerFactory)
        {
            _requestValidator = requestValidator;
            _logger = loggerFactory.CreateLogger<TokenEndpoint>();
        }

        public Task ProcessAsync(HttpContext context)
        {
            _logger.LogVerbose("Start token request.");

            // read input params

            // send to validator

            // send validation result to response generator

            // write out response
            
            return Task.FromResult(0);
        }
    }
}