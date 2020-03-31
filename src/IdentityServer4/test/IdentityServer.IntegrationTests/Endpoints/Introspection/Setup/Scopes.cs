﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentityServer.IntegrationTests.Endpoints.Introspection.Setup
{
    internal class Scopes
    {
        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource
                {
                    Name = "api1",
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    Scopes = { "api1" }
                },
                new ApiResource
                {
                    Name = "api2",
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    Scopes = { "api2" }
                },
                 new ApiResource
                {
                    Name = "api3",
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    Scopes = { "api3-a", "api3-b" }
                }
            };
        }
        public static IEnumerable<ApiScope> GetScopes()
        {
            return new ApiScope[]
            {
                new ApiScope
                {
                    Name = "api1"
                },
                new ApiScope
                {
                    Name = "api2"
                },
                new ApiScope
                {
                    Name = "api3-a"
                },
                new ApiScope
                {
                    Name = "api3-b"
                }
            };
        }
    }
}