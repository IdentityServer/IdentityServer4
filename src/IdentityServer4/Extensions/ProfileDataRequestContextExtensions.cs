// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Models
{
    public static class ProfileDataRequestContextExtensions
    {
        public static List<Claim> FilterClaims(this ProfileDataRequestContext context, IEnumerable<Claim> claims)
        {
            var result = new List<Claim>(claims);
            if (!context.AllClaimsRequested)
            {
                result = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
            }
            return result;
        }

        public static void AddFilteredClaims(this ProfileDataRequestContext context, IEnumerable<Claim> claims)
        {
            context.IssuedClaims.AddRange(context.FilterClaims(claims));
        }
    }
}