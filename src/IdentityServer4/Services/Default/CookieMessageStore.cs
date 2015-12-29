using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task Delete(string id)
        {
            _cookie.Clear(id);
            return Task.FromResult(0);
        }

        public Task<TMessage> Read(string id)
        {
            return Task.FromResult(_cookie.Read(id));
        }

        public Task<string> Write(TMessage message)
        {
            return Task.FromResult(_cookie.Write(message));
        }
    }
}
