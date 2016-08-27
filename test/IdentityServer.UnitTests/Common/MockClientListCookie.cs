// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace IdentityServer4.UnitTests.Common
{
    class MockClientListCookie : ClientListCookie
    {
        public List<string> Clients = new List<string>();

        public MockClientListCookie(IdentityServerOptions options, IHttpContextAccessor context)
            : base(options, context)
        {
        }

        public override void AddClient(string clientId)
        {
            Clients.Add(clientId);
            base.AddClient(clientId);
        }
    }
}
