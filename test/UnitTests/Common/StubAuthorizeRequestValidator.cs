using IdentityServer4.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Security.Claims;

namespace UnitTests.Common
{
    public class StubAuthorizeRequestValidator : IAuthorizeRequestValidator
    {
        private readonly AuthorizeRequestValidationResult _result;

        public StubAuthorizeRequestValidator(AuthorizeRequestValidationResult result)
        {
            _result = result;
        }

        public Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null)
        {
            return Task.FromResult(_result);
        }
    }
}
