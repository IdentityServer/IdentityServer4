using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static IdentityServer4.IdentityServerConstants;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validator for an X.509 certificate based client secret using the common name
    /// </summary>
    public class X509NameSecretValidator : ISecretValidator
    {
        private readonly ILogger<X509NameSecretValidator> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        public X509NameSecretValidator(ILogger<X509NameSecretValidator> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
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
                throw new InvalidOperationException("Credential is not a x509 certificate.");
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
                        { "x5t#S256", cert.Thumbprint }
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
