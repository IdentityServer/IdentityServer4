// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Core.Models
{
    public class RequestedClaimTypes
    {
        public RequestedClaimTypes()
        {
            ClaimTypes = Enumerable.Empty<string>();
        }

        public RequestedClaimTypes(IEnumerable<string> claimTypes)
        {
            ClaimTypes = claimTypes;
        }

        public bool IncludeAllClaims { get; set; }
        public IEnumerable<string> ClaimTypes { get; set; }
    }
}