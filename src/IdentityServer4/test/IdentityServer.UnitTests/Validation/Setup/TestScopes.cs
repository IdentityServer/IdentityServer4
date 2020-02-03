﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentityServer.UnitTests.Validation.Setup
{
    internal class TestScopes
    {
        public static IEnumerable<IdentityResource> GetIdentity()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource
                {
                    Name = "api",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "resource",
                            Description = "resource scope"
                        },
                        new Scope
                        {
                            Name = "resource2",
                            Description = "resource scope"
                        }
                    }
                }
            };
        }
    }
}