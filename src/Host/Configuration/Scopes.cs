// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;

namespace Host.Configuration
{
    public class Scopes
    {
        public static Resources GetResources()
        {
            var identity = new IdentityResource[]
            {
                StandardScopes.OpenId,
                StandardScopes.ProfileAlwaysInclude,
                StandardScopes.EmailAlwaysInclude,
                StandardScopes.RolesAlwaysInclude,
            };

            var api = new ApiResource[]
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

            return new Resources(identity, api);
        }
    }
}