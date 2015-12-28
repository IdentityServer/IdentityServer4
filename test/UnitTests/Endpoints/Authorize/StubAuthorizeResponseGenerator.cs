using IdentityServer4.Core.ResponseHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Validation;
using System.Security.Claims;

namespace UnitTests.Endpoints.Authorize
{
    class StubAuthorizeResponseGenerator : IAuthorizeResponseGenerator
    {
        public AuthorizeResponse Response { get; set; } = new AuthorizeResponse();

        public Task<AuthorizeResponse> CreateResponseAsync(ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(Response);
        }
    }
}
