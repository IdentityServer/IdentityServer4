// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using IdentityModel;

namespace IdentityServer4.Core.Validation
{
    /// <summary>
    /// Parses a POST body for secrets
    /// </summary>
    public class PostBodySecretParser : ISecretParser
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Creates the parser with options
        /// </summary>
        /// <param name="options">IdentityServer options</param>
        public PostBodySecretParser(IdentityServerOptions options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PostBodySecretParser>();
            _options = options;
        }

        public string AuthenticationMethod
        {
            get
            {
                return OidcConstants.EndpointAuthenticationMethods.PostBody;
            }
        }

        /// <summary>
        /// Tries to find a secret on the environment that can be used for authentication
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>
        /// A parsed secret
        /// </returns>
        public Task<ParsedSecret> ParseAsync(HttpContext context)
        {
            _logger.LogVerbose("Start parsing for secret in post body");

            if (!context.Request.HasFormContentType)
            {
                _logger.LogWarning("Content type is not a form");
                return Task.FromResult<ParsedSecret>(null);
            }

            var body = context.Request.Form;

            if (body != null)
            {
                var id = body["client_id"].FirstOrDefault();
                var secret = body["client_secret"].FirstOrDefault();

                if (id.IsPresent() && secret.IsPresent())
                {
                    if (id.Length > _options.InputLengthRestrictions.ClientId ||
                        secret.Length > _options.InputLengthRestrictions.ClientSecret)
                    {
                        _logger.LogWarning("Client ID or secret exceeds maximum lenght.");
                        return Task.FromResult<ParsedSecret>(null);
                    }

                    var parsedSecret = new ParsedSecret
                    {
                        Id = id,
                        Credential = secret,
                        Type = Constants.ParsedSecretTypes.SharedSecret
                    };

                    return Task.FromResult(parsedSecret);
                }
            }

            _logger.LogVerbose("No secret in post body found");
            return Task.FromResult<ParsedSecret>(null);
        }
    }
}