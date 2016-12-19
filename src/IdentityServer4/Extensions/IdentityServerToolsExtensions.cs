using IdentityModel;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Extensions
{
    public static class IdentityServerToolsExtensions
    {
        public static async Task<string> IssueClientJwtAsync(this IdentityServerTools tools, string clientId, int lifetime, IEnumerable<string> scopes = null, IEnumerable<string> audiences = null)
        {
            var claims = new HashSet<Claim>(new ClaimComparer());
            claims.Add(new Claim(JwtClaimTypes.ClientId, clientId));

            if (!scopes.IsNullOrEmpty())
            {
                foreach (var scope in scopes)
                {
                    claims.Add(new Claim(JwtClaimTypes.Scope, scope));
                }
            }

            claims.Add(new Claim(JwtClaimTypes.Audience, string.Format(Constants.AccessTokenAudience, tools._contextAccessor.HttpContext.GetIdentityServerIssuerUri().EnsureTrailingSlash())));
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