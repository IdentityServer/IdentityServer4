// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Claims;

namespace IdentityServer4.Core.Extensions
{
    internal static class ClaimExtensions
    {
        public static bool HasValue(this Claim claim)
        {
            return (claim != null && claim.Value.IsPresent());
        }
    }
}