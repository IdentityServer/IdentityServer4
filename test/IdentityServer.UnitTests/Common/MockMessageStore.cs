// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests.Common
{
    public class MockMessageStore<TModel> : IMessageStore<TModel>
    {
        public Dictionary<string, Message<TModel>> Messages { get; set; } = new Dictionary<string, Message<TModel>>();

        public Task DeleteAsync(string id)
        {
            if (Messages.ContainsKey(id))
            {
                Messages.Remove(id);
            }
            return Task.FromResult(0);
        }

        public Task<Message<TModel>> ReadAsync(string id)
        {
            Message<TModel> val;
            Messages.TryGetValue(id, out val);
            return Task.FromResult(val);
        }

        public Task WriteAsync(Message<TModel> message)
        {
            Messages[message.Id] = message;
            return Task.FromResult(0);
        }
    }
}
