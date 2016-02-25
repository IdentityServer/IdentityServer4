using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    internal interface IEndSessionRequestValidator
    {
        Task<EndSessionRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null);
    }
}
