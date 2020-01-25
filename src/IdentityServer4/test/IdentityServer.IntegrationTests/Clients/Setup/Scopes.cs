// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentityServer.IntegrationTests.Clients.Setup
{
    internal class Scopes
    {
        public static IEnumerable<IdentityResource> GetIdentityScopes()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Address(),
                new IdentityResource("roles", new[] { "role" })
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = "api",
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    Scopes = { "api1", "api2", "api3", "api4.with.roles" }
                },
                new ApiResource("other_api")
                {
                    Scopes = { "other_api" }
                }
            };
        }

        public static IEnumerable<Scope> GetScopes()
        {
            return new Scope[]
            {
                new Scope
                {
                    Name = "api1"
                },
                new Scope
                {
                    Name = "api2"
                },
                new Scope
                {
                    Name = "api3"
                },
                new Scope
                {
                    Name = "api4.with.roles",
                    UserClaims = { "role" }
                },
                new Scope
                {
                    Name = "other_api",
                },
            };
        }
    }
}