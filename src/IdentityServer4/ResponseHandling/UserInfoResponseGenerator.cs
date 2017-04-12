﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
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
        /// Processes the response.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="scopes">The scopes.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Profile service returned incorrect subject value</exception>
        public async Task<Dictionary<string, object>> ProcessAsync(ClaimsPrincipal subject, IEnumerable<string> scopes, Client client)
        {
            _logger.LogTrace("Creating userinfo response");

            var requestedClaimTypes = await GetRequestedClaimTypesAsync(scopes);

            _logger.LogDebug("Requested claim types: {claimTypes}", requestedClaimTypes.ToSpaceSeparatedString());

            var context = new ProfileDataRequestContext(
                subject,
                client,
                IdentityServerConstants.ProfileDataCallers.UserInfoEndpoint,
                requestedClaimTypes);

            await _profile.GetProfileDataAsync(context);
            var profileClaims = context.IssuedClaims;

            List<Claim> results = new List<Claim>();

            if (profileClaims == null)
            {
                _logger.LogInformation("Profile service returned no claims (null)");
            }
            else
            {
                results.AddRange(profileClaims);
                _logger.LogInformation("Profile service returned to the following claim types: {types}", profileClaims.Select(c => c.Type).ToSpaceSeparatedString());
            }

            var subClaim = results.SingleOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (subClaim == null)
            {
                results.Add(new Claim(JwtClaimTypes.Subject, subject.GetSubjectId()));
            }
            else if (subClaim.Value != subject.GetSubjectId())
            {
                _logger.LogError("Profile service returned incorrect subject value: {sub}", subClaim);
                throw new InvalidOperationException("Profile service returned incorrect subject value");
            }

            return results.ToClaimsDictionary();
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