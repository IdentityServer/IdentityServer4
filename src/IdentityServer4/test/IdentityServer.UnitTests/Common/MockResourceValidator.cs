using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common
{
    class MockResourceValidator : IResourceValidator
    {
        public ResourceValidationResult Result { get; set; } = new ResourceValidationResult();

        public Task<ResourceValidationResult> ValidateRequestedResourcesAsync(ResourceValidationRequest request)
        {
            return Task.FromResult(Result);
        }
    }
}
