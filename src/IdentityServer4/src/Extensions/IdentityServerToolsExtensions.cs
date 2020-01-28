// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Configuration;

namespace IdentityServer4
{
    /// <summary>
    /// Extensions for IdentityServerTools
    /// </summary>
    public static class IdentityServerToolsExtensions
    {
        /// <summary>
        /// Issues the client JWT.
        /// </summary>
        /// <param name="tools">The tools.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="lifetime">The lifetime.</param>
        /// <param name="scopes">The scopes.</param>
        /// <param name="audiences">The audiences.</param>
        /// <param name="additionalClaims">Additional claims</param>
        /// <returns></returns>
        public static async Task<string> IssueClientJwtAsync(this IdentityServerTools tools,
            string clientId,
            int lifetime,
            IEnumerable<string> scopes = null,
            IEnumerable<string> audiences = null,
            IEnumerable<Claim> additionalClaims = null)
        {
            var claims = new HashSet<Claim>(new ClaimComparer());
            var context = tools.ContextAccessor.HttpContext;
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            
            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    claims.Add(claim);
                }
            }

            claims.Add(new Claim(JwtClaimTypes.ClientId, clientId));

            if (!scopes.IsNullOrEmpty())
            {
                foreach (var scope in scopes)
                {
                    claims.Add(new Claim(JwtClaimTypes.Scope, scope));
                }
            }

            if (options.EmitLegacyResourceAudienceClaim)
            {
                claims.Add(new Claim(JwtClaimTypes.Audience, string.Format(IdentityServerConstants.AccessTokenAudience, tools.ContextAccessor.HttpContext.GetIdentityServerIssuerUri().EnsureTrailingSlash())));
            }
            
            if (!audiences.IsNullOrEmpty())
            {
                foreach (var audience in audiences)
                {
                    claims.Add(new Claim(JwtClaimTypes.Audience, audience));
                }
            }

            return await tools.IssueJwtAsync(lifetime, claims);
        }
    }
}