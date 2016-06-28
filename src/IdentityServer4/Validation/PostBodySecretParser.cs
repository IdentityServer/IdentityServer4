// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using IdentityModel;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Validation
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
        public PostBodySecretParser(IdentityServerOptions options, ILogger<PostBodySecretParser> logger)
        {
            _logger = logger;
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
            _logger.LogDebug("Start parsing for secret in post body");

            if (!context.Request.HasFormContentType)
            {
                _logger.LogDebug("Content type is not a form");
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
                        _logger.LogError("Client ID or secret exceeds maximum lenght.");
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

            _logger.LogDebug("No secret in post body found");
            return Task.FromResult<ParsedSecret>(null);
        }
    }
}