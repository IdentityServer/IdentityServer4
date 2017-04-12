// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// The introspection response generator
    /// </summary>
    /// <seealso cref="IdentityServer4.ResponseHandling.IIntrospectionResponseGenerator" />
    public class IntrospectionResponseGenerator : IIntrospectionResponseGenerator
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectionResponseGenerator"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public IntrospectionResponseGenerator(ILogger<IntrospectionResponseGenerator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="apiResource">The API resource.</param>
        /// <returns></returns>
        public Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult, ApiResource apiResource)
        {
            _logger.LogTrace("Creating introspection response");

            if (validationResult.IsActive == false)
            {
                _logger.LogDebug("Creating introspection response for inactive token.");

                var response = new Dictionary<string, object>
                {
                    { "active", false }
                };

                return Task.FromResult(response);
            }
            else
            { 
                _logger.LogDebug("Creating introspection response for active token.");

                var response = validationResult.Claims.Where(c => c.Type != JwtClaimTypes.Scope).ToClaimsDictionary();

                var allowedScopes = apiResource.Scopes.Select(x => x.Name);
                var scopes = validationResult.Claims.Where(c => c.Type == JwtClaimTypes.Scope).Select(x => x.Value);
                scopes = scopes.Where(x => allowedScopes.Contains(x));
                response.Add("scope", scopes.ToSpaceSeparatedString());

                response.Add("active", true);

                return Task.FromResult(response);
            }
        }
    }
}