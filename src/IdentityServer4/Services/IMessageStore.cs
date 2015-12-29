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
        Task<string> Write(TMessage message);
        Task<TMessage> Read(string id);
        Task Delete(string id);
    }
}
