using IdentityServer4.Core.Models;
using System.Collections.Generic;

namespace IdentityServer4.Tests.Endpoints.Authorize
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
                    Flow = Flows.Implicit,
                    AllowedScopes = new List<string> { "openid", "profile" },
                    RedirectUris = new List<string> { "https://client1/callback" }
                },
            };
        }
    }
}