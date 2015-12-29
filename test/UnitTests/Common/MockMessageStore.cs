using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests.Common
{
    public class MockMessageStore<TMessage> : IMessageStore<TMessage>
        where TMessage : Message
    {
        public Dictionary<string, TMessage> Messages { get; set; } = new Dictionary<string, TMessage>();

        public Task DeleteAsync(string id)
        {
            if (Messages.ContainsKey(id))
            {
                Messages.Remove(id);
            }
            return Task.FromResult(0);
        }

        public Task<TMessage> ReadAsync(string id)
        {
            TMessage val;
            Messages.TryGetValue(id, out val);
            return Task.FromResult(val);
        }

        public Task<string> WriteAsync(TMessage message)
        {
            var id = IdentityModel.CryptoRandom.CreateUniqueId();
            Messages[id] = message;
            return Task.FromResult(id);
        }
    }
}
