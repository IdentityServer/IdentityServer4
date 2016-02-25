using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Validation
{
    public class EndSessionRequestValidationResult : ValidationResult
    {
        public Client Client { get; set; }
        public string PostLogoutUri { get; set; }
        public string State { get; set; }
    }
}
