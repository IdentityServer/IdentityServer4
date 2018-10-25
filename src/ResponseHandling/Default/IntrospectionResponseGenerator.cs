// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
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
        /// <summary>
        /// Gets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        protected readonly IEventService Events;

        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectionResponseGenerator" /> class.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        public IntrospectionResponseGenerator(IEventService events, ILogger<IntrospectionResponseGenerator> logger)
        {
            Events = events;
            Logger = logger;
        }

        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult)
        {
            Logger.LogTrace("Creating introspection response");

            // standard response
            var response = new Dictionary<string, object>
            {
                { "active", false }
            };

            // token is invalid
            if (validationResult.IsActive == false)
            {
                Logger.LogDebug("Creating introspection response for inactive token.");
                await Events.RaiseAsync(new TokenIntrospectionSuccessEvent(validationResult));

                return response;
            }

            // expected scope not present
            if (await AreExpectedScopesPresentAsync(validationResult) == false)
            {
                return response;
            }

            Logger.LogDebug("Creating introspection response for active token.");

            // get all claims (without scopes)
            response = validationResult.Claims.Where(c => c.Type != JwtClaimTypes.Scope).ToClaimsDictionary();

            // add active flag
            response.Add("active", true);

            // calculate scopes the caller is allowed to see
            var allowedScopes = validationResult.Api.Scopes.Select(x => x.Name);
            var scopes = validationResult.Claims.Where(c => c.Type == JwtClaimTypes.Scope).Select(x => x.Value);
            scopes = scopes.Where(x => allowedScopes.Contains(x));
            response.Add("scope", scopes.ToSpaceSeparatedString());

            await Events.RaiseAsync(new TokenIntrospectionSuccessEvent(validationResult));
            return response;
        }

        /// <summary>
        /// Checks if the API resource is allowed to introspect the scopes.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns></returns>
        protected virtual async Task<bool> AreExpectedScopesPresentAsync(IntrospectionRequestValidationResult validationResult)
        {
            var apiScopes = validationResult.Api.Scopes.Select(x => x.Name);
            var tokenScopes = validationResult.Claims.Where(c => c.Type == JwtClaimTypes.Scope);

            var tokenScopesThatMatchApi = tokenScopes.Where(c => apiScopes.Contains(c.Value));

            var result = false;

            if (tokenScopesThatMatchApi.Any())
            {
                // at least one of the scopes the API supports is in the token
                result = true;
            }
            else
            {
                // no scopes for this API are found in the token
                Logger.LogError("Expected scope {scopes} is missing in token", apiScopes);
                await Events.RaiseAsync(new TokenIntrospectionFailureEvent(validationResult.Api.Name, "Expected scopes are missing", validationResult.Token, apiScopes, tokenScopes.Select(s => s.Value)));
            }

            return result;
        }
    }
}