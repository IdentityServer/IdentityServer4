using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Host.Mtls
{
    public class MtlsSecretParser : ISecretParser
    {
        private readonly IdentityServerOptions _options;
        private readonly ILogger<MtlsSecretParser> _logger;

        public MtlsSecretParser(IdentityServerOptions options, ILogger<MtlsSecretParser> logger)
        {
            _options = options;
            _logger = logger;
        }

        public string AuthenticationMethod => "mTLS";

        public async Task<ParsedSecret> ParseAsync(HttpContext context)
        {
            _logger.LogDebug("Start parsing for client id in post body");

            if (!context.Request.HasFormContentType)
            {
                _logger.LogDebug("Content type is not a form");
                return null;
            }

            var body = await context.Request.ReadFormAsync();

            if (body != null)
            {
                var id = body["client_id"].FirstOrDefault();

                // client id must be present
                if (!String.IsNullOrWhiteSpace(id))
                {
                    if (id.Length > _options.InputLengthRestrictions.ClientId)
                    {
                        _logger.LogError("Client ID exceeds maximum length.");
                        return null;
                    }

                    if (!context.Items.ContainsKey(MtlsConstants.X509CertificateItemKey))
                    {
                        _logger.LogDebug("Client certificate not present");
                        return null;
                    }

                    var cert = context.Items[MtlsConstants.X509CertificateItemKey] as X509Certificate2;
                    if (cert == null)
                    {
                        _logger.LogDebug("Client certificate invalid from items collection");
                        return null;
                    }

                    return new ParsedSecret
                    {
                        Id = id,
                        Credential = cert,
                        Type = IdentityServerConstants.ParsedSecretTypes.X509Certificate
                    };
                }
            }

            _logger.LogDebug("No post body found");
            return null;
        }
    }
}
