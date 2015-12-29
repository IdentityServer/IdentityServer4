using IdentityServer4.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    public interface IMessageStore<TMessage>
        where TMessage : Message
    {
        Task<string> WriteAsync(TMessage message);
        Task<TMessage> ReadAsync(string id);
        Task DeleteAsync(string id);
    }
}
