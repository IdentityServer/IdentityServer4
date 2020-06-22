using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common
{
    class MockReferenceTokenStore : IReferenceTokenStore
    {
        public Task<Token> GetReferenceTokenAsync(string handle, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReferenceTokenAsync(string handle, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReferenceTokensAsync(string subjectId, string clientId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> StoreReferenceTokenAsync(Token token, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
