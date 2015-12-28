using IdentityServer4.Core.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests.Common
{
    class MockClientListCookie : ClientListCookie
    {
        public List<string> Clients = new List<string>();

        public MockClientListCookie(IdentityServerContext context)
            : base(context)
        {
        }

        public override void AddClient(string clientId)
        {
            Clients.Add(clientId);
            base.AddClient(clientId);
        }
    }
}
