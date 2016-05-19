// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using System.Collections.Generic;

namespace IdentityServer4.Tests.Validation
{
    class TestScopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new Scope[]
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.OfflineAccess,

                new Scope
                {
                    Name = "resource",
                    Description = "resource scope",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "resource2",
                    Description = "resource scope",
                    Type = ScopeType.Resource
                },
             };
        }
    }
}