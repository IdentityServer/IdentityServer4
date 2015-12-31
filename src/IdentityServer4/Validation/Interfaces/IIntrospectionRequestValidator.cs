using System.Collections.Specialized;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;

namespace IdentityServer4.Core.Validation
{
    public interface IIntrospectionRequestValidator
    {
        Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, Scope scope);
    }
}