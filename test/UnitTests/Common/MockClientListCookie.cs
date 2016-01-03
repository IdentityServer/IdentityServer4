// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Hosting;
using System.Collections.Generic;

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
