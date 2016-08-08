using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer.UnitTests.Validation.EndSessionRequestValidation
{
    public class StubRedirectUriValidator : IRedirectUriValidator
    {
        public bool IsRedirectUriValid { get; set; }
        public bool IsPostLogoutRedirectUriValid { get; set; }

        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(IsPostLogoutRedirectUriValid);
        }

        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(IsRedirectUriValid);
        }
    }
}
