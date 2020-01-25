using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
