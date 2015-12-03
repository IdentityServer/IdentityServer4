using IdentityServer4.Core.Results;
using IdentityServer4.Core.Validation;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
{
    public class TokenEndpoint : IEndpoint
    {
        private readonly ClientSecretValidator _clientValidator;
        private readonly ILogger _logger;
        private readonly TokenRequestValidator _requestValidator;

        public TokenEndpoint(TokenRequestValidator requestValidator, ClientSecretValidator clientValidator, ILoggerFactory loggerFactory)
        {
            _requestValidator = requestValidator;
            _clientValidator = clientValidator;
            _logger = loggerFactory.CreateLogger<TokenEndpoint>();
        }

        public async Task<IResult> ProcessAsync(HttpContext context)
        {
            _logger.LogVerbose("Start token request.");

            var clientResult = await _clientValidator.ValidateAsync(context);

            if (clientResult.Client == null)
            {
                return new TokenErrorResult("invalid_client");
            }

            // read input params

            // send to validator

            // send validation result to response generator

            // write out response

            return null;
        }
    }
}