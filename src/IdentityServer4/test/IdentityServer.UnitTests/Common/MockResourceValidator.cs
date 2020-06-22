using IdentityServer4.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common
{
    class MockResourceValidator : IResourceValidator
    {
        public ResourceValidationResult Result { get; set; } = new ResourceValidationResult();

        public Task<IEnumerable<ParsedScopeValue>> ParseRequestedScopesAsync(IEnumerable<string> scopeValues, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(scopeValues.Select(x => new ParsedScopeValue(x)));
        }

        public Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result);
        }
    }
}
