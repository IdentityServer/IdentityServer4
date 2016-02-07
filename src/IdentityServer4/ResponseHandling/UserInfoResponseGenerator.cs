// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    public class UserInfoResponseGenerator : IUserInfoResponseGenerator
    {
        private readonly ILogger _logger;
        private readonly IProfileService _profile;
        private readonly IScopeStore _scopes;

        public UserInfoResponseGenerator(IProfileService profile, IScopeStore scopes, ILogger<UserInfoResponseGenerator> logger)
        {
            _profile = profile;
            _scopes = scopes;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> ProcessAsync(string subject, IEnumerable<string> scopes, Client client)
        {
            _logger.LogVerbose("Creating userinfo response");

            var profileData = new Dictionary<string, object>();
            
            var requestedClaimTypes = await GetRequestedClaimTypesAsync(scopes);
            var principal = Principal.Create("UserInfo", new Claim("sub", subject));

            IEnumerable<Claim> profileClaims;
            if (requestedClaimTypes.IncludeAllClaims)
            {
                _logger.LogInformation("Requested claim types: all");

                var context = new ProfileDataRequestContext(
                    principal, 
                    client, 
                    Constants.ProfileDataCallers.UserInfoEndpoint);

                await _profile.GetProfileDataAsync(context);
                profileClaims = context.IssuedClaims;
            }
            else
            {
                _logger.LogInformation("Requested claim types: {types}", requestedClaimTypes.ClaimTypes.ToSpaceSeparatedString());

                var context = new ProfileDataRequestContext(
                    principal,
                    client,
                    Constants.ProfileDataCallers.UserInfoEndpoint,
                    requestedClaimTypes.ClaimTypes);

                await _profile.GetProfileDataAsync(context);
                profileClaims = context.IssuedClaims;
            }
            
            if (profileClaims != null)
            {
                profileData = profileClaims.ToClaimsDictionary();
                _logger.LogInformation("Profile service returned to the following claim types: {types}", profileClaims.Select(c => c.Type).ToSpaceSeparatedString());
            }
            else
            {
                _logger.LogInformation("Profile service returned no claims (null)");
            }

            return profileData;
        }

        public async Task<RequestedClaimTypes> GetRequestedClaimTypesAsync(IEnumerable<string> scopes)
        {
            if (scopes == null || !scopes.Any())
            {
                return new RequestedClaimTypes();
            }

            var scopeString = string.Join(" ", scopes);
            _logger.LogInformation("Scopes in access token: {scopes}", scopeString);

            var scopeDetails = await _scopes.FindScopesAsync(scopes);
            var scopeClaims = new List<string>();

            foreach (var scope in scopes)
            {
                var scopeDetail = scopeDetails.FirstOrDefault(s => s.Name == scope);
                
                if (scopeDetail != null)
                {
                    if (scopeDetail.Type == ScopeType.Identity)
                    {
                        if (scopeDetail.IncludeAllClaimsForUser)
                        {
                            return new RequestedClaimTypes
                            {
                                IncludeAllClaims = true
                            };
                        }

                        scopeClaims.AddRange(scopeDetail.Claims.Select(c => c.Name));
                    }
                }
            }

            return new RequestedClaimTypes(scopeClaims);
        }
    }
}