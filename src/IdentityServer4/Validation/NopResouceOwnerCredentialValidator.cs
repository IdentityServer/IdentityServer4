using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    public class NopResouceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly ILogger<NopResouceOwnerPasswordValidator> _logger;

        public NopResouceOwnerPasswordValidator(ILogger<NopResouceOwnerPasswordValidator> logger)
        {
            _logger = logger;
        }

        public Task<GrantValidationResult> ValidateAsync(string userName, string password, ValidatedTokenRequest request)
        {
            var result = new GrantValidationResult("unsupported_grant_type");

            _logger.LogWarning("Resource owner password credential type not supported. Configure an IResourceOwnerPasswordValidator.");
            return Task.FromResult(result);
        }
    }
}