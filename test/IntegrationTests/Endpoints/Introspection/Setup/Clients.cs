using IdentityServer4.Core.Models;
using System.Collections.Generic;

namespace IdentityServer4.Tests.Endpoints.Introspection
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
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ClientCredentials,
                    AllowAccessToAllScopes = true,
                    AccessTokenType = AccessTokenType.Reference
                },
                new Client
                {
                    ClientId = "client2",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ClientCredentials,
                    AllowAccessToAllScopes = true,
                    AccessTokenType = AccessTokenType.Reference
                }
            };
        }
    }
}