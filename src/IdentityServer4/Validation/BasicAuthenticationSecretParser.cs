// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using IdentityModel;

namespace IdentityServer4.Core.Validation
{
    /// <summary>
    /// Parses a Basic Authentication header
    /// </summary>
    public class BasicAuthenticationSecretParser : ISecretParser
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Creates the parser with a reference to identity server options
        /// </summary>
        /// <param name="options">IdentityServer options</param>
        public BasicAuthenticationSecretParser(IdentityServerOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _logger = loggerFactory.CreateLogger<BasicAuthenticationSecretParser>();
        }

        public string AuthenticationMethod
        {
            get
            {
                return OidcConstants.EndpointAuthenticationMethods.BasicAuthentication;
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
            _logger.LogVerbose("Start parsing Basic Authentication secret");

            var notfound = Task.FromResult<ParsedSecret>(null);
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (authorizationHeader.IsMissing())
            {
                return notfound;
            }

            if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return notfound;
            }

            var parameter = authorizationHeader.Substring("Basic ".Length);

            string pair;
            try
            {
                pair = Encoding.UTF8.GetString(
                    Convert.FromBase64String(parameter));
            }
            catch (FormatException)
            {
                _logger.LogVerbose("Malformed Basic Authentication credential.");
                return notfound;
            }
            catch (ArgumentException)
            {
                _logger.LogVerbose("Malformed Basic Authentication credential.");
                return notfound;
            }

            var ix = pair.IndexOf(':');
            if (ix == -1)
            {
                _logger.LogVerbose("Malformed Basic Authentication credential.");
                return notfound;
            }

            var clientId = pair.Substring(0, ix);
            var secret = pair.Substring(ix + 1);

            if (clientId.IsPresent() && secret.IsPresent())
            {
                if (clientId.Length > _options.InputLengthRestrictions.ClientId ||
                    secret.Length > _options.InputLengthRestrictions.ClientSecret)
                {
                    _logger.LogError("Client ID or secret exceeds allowed length.");
                    return notfound;
                }

                var parsedSecret = new ParsedSecret
                {
                    Id = clientId,
                    Credential = secret,
                    Type = Constants.ParsedSecretTypes.SharedSecret
                };

                return Task.FromResult(parsedSecret);
            }

            _logger.LogVerbose("No Basic Authentication secret found");
            return notfound;
        }
    }
}