// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services.InMemory;
using System.Collections.Generic;

namespace IdentityServer4.IntegrationTests.Endpoints.Introspection
{
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Subject = "1",
                    Username = "bob",
                    Password = "bob"
                }
            };
        }
    }
}