// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Threading.Tasks;
using System;

namespace IdentityServer4.Services
{
    public interface IMessageStore<TModel>
    {
        Task WriteAsync(string id, Message<TModel> message);
        Task<Message<TModel>> ReadAsync(string id);
        Task DeleteAsync(string id);
    }
}
