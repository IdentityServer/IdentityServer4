using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.ResponseHandling;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Events;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Endpoints
{
    public class UserInfoEndpoint : IEndpoint
    {
        private readonly ILogger _logger;
        private readonly IEventService _events;
        private readonly IUserInfoResponseGenerator _generator;
        private readonly IdentityServerOptions _options;
        private readonly BearerTokenUsageValidator _tokenUsageValidator;
        private readonly ITokenValidator _tokenValidator;

        public UserInfoEndpoint(IdentityServerOptions options, ITokenValidator tokenValidator, IUserInfoResponseGenerator generator, BearerTokenUsageValidator tokenUsageValidator, IEventService events, ILogger<UserInfoEndpoint> logger)
        {
            _options = options;
            _tokenValidator = tokenValidator;
            _tokenUsageValidator = tokenUsageValidator;
            _generator = generator;
            _events = events;
            _logger = logger;
        }

        // todo: no caching
        public async Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            if (context.HttpContext.Request.Method != "GET" && context.HttpContext.Request.Method != "POST")
            {
                return new StatusCodeResult(405);
            }

            _logger.LogVerbose("Start userinfo request");

            var tokenUsageResult = await _tokenUsageValidator.ValidateAsync(context.HttpContext);
            if (tokenUsageResult.TokenFound == false)
            {
                var error = "No token found.";

                _logger.LogError(error);
                await RaiseFailureEventAsync(error);
                return Error(Constants.ProtectedResourceErrors.InvalidToken);
            }

            _logger.LogInformation("Token found: {token}", tokenUsageResult.UsageType.ToString());

            var tokenResult = await _tokenValidator.ValidateAccessTokenAsync(
                tokenUsageResult.Token,
                Constants.StandardScopes.OpenId);

            if (tokenResult.IsError)
            {
                _logger.LogError(tokenResult.Error);
                await RaiseFailureEventAsync(tokenResult.Error);
                return Error(tokenResult.Error);
            }

            // pass scopes/claims to profile service
            var subject = tokenResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject).Value;
            var scopes = tokenResult.Claims.Where(c => c.Type == Constants.ClaimTypes.Scope).Select(c => c.Value);

            var payload = await _generator.ProcessAsync(subject, scopes, tokenResult.Client);

            _logger.LogInformation("End userinfo request");
            await RaiseSuccessEventAsync();

            return new UserInfoResult(payload);
        }

        private IEndpointResult Error(string error, string description = null)
        {
            return new ProtectedResourceErrorResult(error, description);
        }

        private async Task RaiseSuccessEventAsync()
        {
            await _events.RaiseSuccessfulEndpointEventAsync(EventConstants.EndpointNames.UserInfo);
        }

        private async Task RaiseFailureEventAsync(string error)
        {
            if (_options.EventsOptions.RaiseFailureEvents)
            {
                await _events.RaiseFailureEndpointEventAsync(EventConstants.EndpointNames.UserInfo, error);
            }
        }
    }
}
