// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading.Tasks;
using System.Collections.Specialized;
using IdentityServer4.Extensions;

namespace IdentityServer4.Stores
{
    // internal just for testing
    internal class QueryStringAuthorizationParametersMessageStore : IAuthorizationParametersMessageStore
    {
        public Task<string> WriteAsync(Message<NameValueCollection> message)
        {
            var queryString = message.Data.ToQueryString();
            return Task.FromResult(queryString);
        }

        public Task<Message<NameValueCollection>> ReadAsync(string id)
        {
            var values = id.ReadQueryStringAsNameValueCollection();
            var msg = new Message<NameValueCollection>(values);
            return Task.FromResult(msg);
        }

        public Task DeleteAsync(string id)
        {
            return Task.CompletedTask;
        }
    }
}
