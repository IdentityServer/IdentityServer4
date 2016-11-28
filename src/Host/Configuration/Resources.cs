// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace Host.Configuration
{
    public class Resources
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                StandardScopes.OpenId,
                StandardScopes.ProfileAlwaysInclude,
                StandardScopes.EmailAlwaysInclude,
                StandardScopes.RolesAlwaysInclude,
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new ApiResource[]
            {
                new ApiResource
                {
                    Name = "api1",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "api1",
                            DisplayName = "API 1",
                            Description = "Some API 1 Description"
                        }
                    },
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    UserClaims =
                    {
                        new ScopeClaim("role")
                    }
                },
                new ApiResource
                {
                    Name = "api2",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "api2",
                            DisplayName = "API 2",
                            Description = "Some API 2 Description"
                        }
                    }
                }
            };
        }
    }
}