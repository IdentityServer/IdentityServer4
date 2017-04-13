// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// The userinfo response generator
    /// </summary>
    /// <seealso cref="IdentityServer4.ResponseHandling.IUserInfoResponseGenerator" />
    public class UserInfoResponseGenerator : IUserInfoResponseGenerator
    {
        private readonly ILogger _logger;
        private readonly IProfileService _profile;
        private readonly IResourceStore _resourceStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoResponseGenerator"/> class.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <param name="resourceStore">The resource store.</param>
        /// <param name="logger">The logger.</param>
        public UserInfoResponseGenerator(IProfileService profile, IResourceStore resourceStore, ILogger<UserInfoResponseGenerator> logger)
        {
            _profile = profile;
            _resourceStore = resourceStore;
            _logger = logger;
        }

        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="validationResult">The userinfo request validation result.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Profile service returned incorrect subject value</exception>
        public async Task<Dictionary<string, object>> ProcessAsync(UserInfoRequestValidationResult validationResult)
        {
            _logger.LogTrace("Creating userinfo response");

            // extract scopes and turn into requested claim types
            var scopes = validationResult.Subject.Claims.Where(c => c.Type == JwtClaimTypes.Scope).Select(c => c.Value);
            var requestedClaimTypes = await GetRequestedClaimTypesAsync(scopes);

            _logger.LogDebug("Requested claim types: {claimTypes}", requestedClaimTypes.ToSpaceSeparatedString());

            // call profile service
            var context = new ProfileDataRequestContext(
                validationResult.Subject,
                validationResult.TokenValidationResult.Client,
                IdentityServerConstants.ProfileDataCallers.UserInfoEndpoint,
                requestedClaimTypes);

            await _profile.GetProfileDataAsync(context);
            var profileClaims = context.IssuedClaims;

            // construct outgoing claims
            List<Claim> outgoingClaims = new List<Claim>();

            if (profileClaims == null)
            {
                _logger.LogInformation("Profile service returned no claims (null)");
            }
            else
            {
                outgoingClaims.AddRange(profileClaims);
                _logger.LogInformation("Profile service returned to the following claim types: {types}", profileClaims.Select(c => c.Type).ToSpaceSeparatedString());
            }

            var subClaim = outgoingClaims.SingleOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (subClaim == null)
            {
                outgoingClaims.Add(new Claim(JwtClaimTypes.Subject, validationResult.Subject.GetSubjectId()));
            }
            else if (subClaim.Value != validationResult.Subject.GetSubjectId())
            {
                _logger.LogError("Profile service returned incorrect subject value: {sub}", subClaim);
                throw new InvalidOperationException("Profile service returned incorrect subject value");
            }

            return outgoingClaims.ToClaimsDictionary();
        }

        /// <summary>
        /// Gets the requested claim types.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetRequestedClaimTypesAsync(IEnumerable<string> scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                return Enumerable.Empty<string>();
            }

            var scopeString = string.Join(" ", scopes);
            _logger.LogDebug("Scopes in access token: {scopes}", scopeString);

            var identityResources = await _resourceStore.FindEnabledIdentityResourcesByScopeAsync(scopes);
            var scopeClaims = new List<string>();

            foreach (var scope in scopes)
            {
                var scopeDetail = identityResources.FirstOrDefault(s => s.Name == scope);
                
                if (scopeDetail != null)
                {
                    scopeClaims.AddRange(scopeDetail.UserClaims);
                }
            }

            return scopeClaims.Distinct();
        }
    }
}