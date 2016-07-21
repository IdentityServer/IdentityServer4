// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Models the result of custom grant validation.
    /// </summary>
    public class GrantValidationResult : ValidationResult
    {
        /// <summary>
        /// Gets or sets the principal which represents the result of the authentication.
        /// </summary>
        /// <value>
        /// The principal.
        /// </value>
        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantValidationResult"/> class with an error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public GrantValidationResult(string errorMessage)
        {
            Error = errorMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantValidationResult"/> class with a given principal.
        /// Warning: the principal needs to include the required claims - it is recommended to use the other constructor that does validation.
        /// </summary>
        public GrantValidationResult(ClaimsPrincipal principal)
        {
            // TODO: more checks on claims (amr, etc...)
            Principal = principal;
            IsError = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrantValidationResult"/> class.
        /// </summary>
        /// <param name="subject">The subject claim used to uniquely identifier the user.</param>
        /// <param name="authenticationMethod">The authentication method which describes the custom grant type.</param>
        /// <param name="claims">Additional claims that will be maintained in the principal.</param>
        /// <param name="identityProvider">The identity provider.</param>
        public GrantValidationResult(
            string subject, 
            string authenticationMethod,
            IEnumerable<Claim> claims = null,
            string identityProvider = Constants.LocalIdentityProvider)
        {
            var resultClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, subject),
                new Claim(JwtClaimTypes.AuthenticationMethod, authenticationMethod),
                new Claim(JwtClaimTypes.IdentityProvider, identityProvider),
                new Claim(JwtClaimTypes.AuthenticationTime, DateTimeOffsetHelper.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer)
            };

            if (!claims.IsNullOrEmpty())
            {
                resultClaims.AddRange(claims);
            }

            var id = new ClaimsIdentity(authenticationMethod);
            id.AddClaims(resultClaims.Distinct(new ClaimComparer()));

            Principal = new ClaimsPrincipal(id);

            IsError = false;
        }
    }
}