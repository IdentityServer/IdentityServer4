﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.UnitTests.Common
{
    public class MockMessageStore<TModel> : IMessageStore<TModel>
    {
        public Dictionary<string, Message<TModel>> Messages { get; set; } = new Dictionary<string, Message<TModel>>();

        public Task DeleteAsync(string id)
        {
            if (id != null && Messages.ContainsKey(id))
            {
                Messages.Remove(id);
            }
            return Task.FromResult(0);
        }

        public Task<Message<TModel>> ReadAsync(string id)
        {
            Message<TModel> val = null;
            if (id != null)
            {
                Messages.TryGetValue(id, out val);
            }
            return Task.FromResult(val);
        }

        public Task WriteAsync(string id, Message<TModel> message)
        {
            Messages[id] = message;
            return Task.FromResult(0);
        }
    }
}
