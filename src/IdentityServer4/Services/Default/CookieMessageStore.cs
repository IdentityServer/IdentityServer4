// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services.Default
{
    internal class CookieMessageStore<TMessage> : IMessageStore<TMessage>
        where TMessage : Message
    {
        private readonly MessageCookie<TMessage> _cookie;

        public CookieMessageStore(MessageCookie<TMessage> cookie)
        {
            _cookie = cookie;
        }

        public Task DeleteAsync(string id)
        {
            _cookie.Clear(id);
            return Task.FromResult(0);
        }

        public Task<TMessage> ReadAsync(string id)
        {
            return Task.FromResult(_cookie.Read(id));
        }

        public Task<string> WriteAsync(TMessage message)
        {
            return Task.FromResult(_cookie.Write(message));
        }
    }
}
