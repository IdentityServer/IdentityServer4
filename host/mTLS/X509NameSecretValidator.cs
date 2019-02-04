using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static IdentityServer4.IdentityServerConstants;

namespace Host.Mtls
{
    public class X509NameSecretValidator : ISecretValidator
    {
        private readonly ILogger<X509NameSecretValidator> _logger;

        public X509NameSecretValidator(ILogger<X509NameSecretValidator> logger)
        {
            _logger = logger;
        }

        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });

            if (parsedSecret.Type != ParsedSecretTypes.X509Certificate)
            {
                _logger.LogDebug("X509 name secret validator cannot process {type}", parsedSecret.Type ?? "null");
                return fail;
            }

            var cert = parsedSecret.Credential as X509Certificate2;
            if (cert == null)
            {
                throw new ArgumentException("Credential is not a x509 certificate.");
            }

            var name = cert.Subject;
            if (name == null)
            {
                _logger.LogWarning("No subject/name found in X509 certificate.");
                return fail;
            }

            var nameSecrets = secrets.Where(s => s.Type == SecretTypes.X509CertificateName);
            if (!nameSecrets.Any())
            {
                _logger.LogDebug("No x509 name secrets configured for client.");
                return fail;
            }

            foreach (var nameSecret in nameSecrets)
            {
                var secretDescription = string.IsNullOrEmpty(nameSecret.Description) ? "no description" : nameSecret.Description;

                if (name.Equals(nameSecret.Value, StringComparison.Ordinal))
                {
                    var values = new Dictionary<string, string>
                    {
                        { "x5t#S256", name }
                    };
                    var cnf = JsonConvert.SerializeObject(values);

                    var result = new SecretValidationResult
                    {
                        Success = true,
                        Confirmation = cnf
                    };

                    return Task.FromResult(result);
                }
            }

            _logger.LogDebug("No matching x509 name secret found.");
            return fail;
        }
    }
}
