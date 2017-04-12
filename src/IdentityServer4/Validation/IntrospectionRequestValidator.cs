﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// The introspection request validator
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.IIntrospectionRequestValidator" />
    public class IntrospectionRequestValidator : IIntrospectionRequestValidator
    {
        private readonly ILogger<IntrospectionRequestValidator> _logger;
        private readonly ITokenValidator _tokenValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectionRequestValidator"/> class.
        /// </summary>
        /// <param name="tokenValidator">The token validator.</param>
        /// <param name="logger">The logger.</param>
        public IntrospectionRequestValidator(ITokenValidator tokenValidator, ILogger<IntrospectionRequestValidator> logger)
        {
            _tokenValidator = tokenValidator;
            _logger = logger;
        }

        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="apiResource">The API resource.</param>
        /// <returns></returns>
        public async Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, ApiResource apiResource)
        {
            _logger.LogDebug("Introspection request validation started.");

            var fail = new IntrospectionRequestValidationResult { IsError = true };

            // retrieve required token
            var token = parameters.Get("token");
            if (token == null)
            {
                _logger.LogError("Token is missing");

                fail.IsActive = false;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.MissingToken;
                return fail;
            }

            // validate token
            var tokenValidationResult = await _tokenValidator.ValidateAccessTokenAsync(token);

            // invalid or unknown token
            if (tokenValidationResult.IsError)
            {
                _logger.LogDebug("Token is invalid.");

                fail.IsActive = false;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.InvalidToken;
                fail.Token = token;
                return fail;
            }

            // check expected scopes
            var supportedScopes = apiResource.Scopes.Select(x => x.Name);
            var expectedScopes = tokenValidationResult.Claims.Where(
                c => c.Type == JwtClaimTypes.Scope && supportedScopes.Contains(c.Value));

            // expected scope not present
            if (!expectedScopes.Any())
            {
                _logger.LogError("Expected scope {scopes} is missing in token", supportedScopes);

                fail.IsActive = false;
                fail.IsError = true;
                fail.FailureReason = IntrospectionRequestValidationFailureReason.InvalidScope;
                fail.Token = token;
                return fail;
            }

            var claims = tokenValidationResult.Claims;

            // filter out scopes that this API resource does not own
            claims = claims.Where(x => x.Type != JwtClaimTypes.Scope ||
                (x.Type == JwtClaimTypes.Scope && supportedScopes.Contains(x.Value)));

            // all is good
            var success = new IntrospectionRequestValidationResult
            {
                IsActive = true,
                IsError = false,
                Token = token,
                Claims = claims
            };

            _logger.LogDebug("Introspection request validation successful.");
            return success;
        }
    }
}