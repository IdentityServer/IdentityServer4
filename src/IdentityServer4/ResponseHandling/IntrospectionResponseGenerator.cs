// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    public class IntrospectionResponseGenerator : IIntrospectionResponseGenerator
    {
        private readonly ILogger _logger;

        public IntrospectionResponseGenerator(ILogger<IntrospectionResponseGenerator> logger)
        {
            _logger = logger;
        }

        public Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult, Scope scope)
        {
            _logger.LogVerbose("Creating introspection response");

            var response = new Dictionary<string, object>();
            
            if (validationResult.IsActive == false)
            {
                _logger.LogInformation("Creating introspection response for inactive token.");

                response.Add("active", false);
                return Task.FromResult(response);
            }

            if (scope.AllowUnrestrictedIntrospection)
            {
                _logger.LogInformation("Creating unrestricted introspection response for active token.");

                response = validationResult.Claims.ToClaimsDictionary();
                response.Add("active", true);
            }
            else
            {
                _logger.LogInformation("Creating restricted introspection response for active token.");

                response = validationResult.Claims.Where(c => c.Type != JwtClaimTypes.Scope).ToClaimsDictionary();
                response.Add("active", true);
                response.Add("scope", scope.Name);
            }

            return Task.FromResult(response);
        }
    }
}