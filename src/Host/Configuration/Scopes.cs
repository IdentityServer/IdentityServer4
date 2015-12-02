using IdentityServer4.Core.Models;
using System.Collections.Generic;

namespace Host.Configuration
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "api1",
                    Type = ScopeType.Resource
                }
            };
        }
    }
}