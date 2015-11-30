using IdentityServer4.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Host.Configuration
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ClientCredentials,

                    AllowedScopes = new List<string>
                    {
                        "api1"
                    }
                }
            };
        }
    }
}