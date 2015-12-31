using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Validation;

namespace IdentityServer4.Core.ResponseHandling
{
    public interface IIntrospectionResponseGenerator
    {
        Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult, Scope scope);
    }
}