using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Host
{
    public class ConfirmationSecretValidator : ISecretValidator
    {
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            if (secrets.First().Type == "confirmation.test")
            {
                var result = new SecretValidationResult
                {
                    Success = true,
                    Confirmation = new Dictionary<string, string>
                    {
                        { "x5t#S256", "foo" }
                    }
                };

                return Task.FromResult(result);
            }

            return Task.FromResult(new SecretValidationResult { Success = false });
        }
    }
}