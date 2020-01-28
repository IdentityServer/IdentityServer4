// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Parses secret according to MTLS spec
    /// </summary>
    public class MutualTlsSecretParser : ISecretParser
    {
        private readonly IdentityServerOptions _options;
        private readonly ILogger<MutualTlsSecretParser> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public MutualTlsSecretParser(IdentityServerOptions options, ILogger<MutualTlsSecretParser> logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Name of authentication method (blank to suppress in discovery since we do special handling)
        /// </summary>
        public string AuthenticationMethod => String.Empty;

        /// <summary>
        /// Parses the HTTP context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
                    
                    var clientCertificate = await context.Connection.GetClientCertificateAsync();
                    
                    if (clientCertificate is null)
                    {
                        _logger.LogDebug("Client certificate not present");
                        return null;
                    }
                    
                    return new ParsedSecret
                    {
                        Id = id,
                        Credential = clientCertificate,
                        Type = IdentityServerConstants.ParsedSecretTypes.X509Certificate
                    };
                }
            }

            _logger.LogDebug("No post body found");
            return null;
        }
    }
}