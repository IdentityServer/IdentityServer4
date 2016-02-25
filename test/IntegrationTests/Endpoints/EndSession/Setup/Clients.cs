// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using System.Collections.Generic;

namespace IdentityServer4.Tests.Endpoints.EndSession
{
    class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client1",
                    RedirectUris = new List<string>
                    {
                        "http://client/index.html"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://client/callback.html"
                    },
                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name
                    },

                    RequireConsent = false,
                    Flow = Flows.Implicit,
                    AccessTokenType = AccessTokenType.Reference
                },
                new Client
                {
                    ClientId = "client2",
                    RedirectUris = new List<string>
                    {
                        "http://client2/callback.html"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://client2/index.html",
                        "http://client2/index2.html"
                    },
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.Hybrid,
                    AccessTokenType = AccessTokenType.Reference
                }
            };
        }
    }
}