// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System;

namespace IdentityServer4.Stores
{
    // flag implementation to skip use because some DI containers don't allow for optional ctor params.
    internal class NopAuthorizationParametersMessageStore : IAuthorizationParametersMessageStore
    {
        public Task<string> WriteAsync(Message<NameValueCollection> message)
        {
            throw new NotImplementedException();
        }

        public Task<Message<NameValueCollection>> ReadAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
