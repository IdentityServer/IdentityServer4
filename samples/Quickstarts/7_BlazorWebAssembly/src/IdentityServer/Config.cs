// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            { 
                new ApiScope("api", "Weather Forecast")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // Blazor WebAssembly Client
                new Client
                {
                    ClientId = "WasmClient-client",
                    ClientName = "Blazor Webassembly App Client",
                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    AllowedCorsOrigins = { "https://localhost:5015" },
                    RedirectUris = { "https://localhost:5015/authentication/login-callback" },
                    PostLogoutRedirectUris = { "https://localhost:5015/authentication/logout-callback" },

                    AllowedScopes = {"openid", "profile", "api" },
                }
            };
    }
}