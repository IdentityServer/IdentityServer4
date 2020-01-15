using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.UnitTests.Common
{
    class MockResourceValidator : IResourceValidator
    {
        public ResourceValidationResult Result { get; set; } = new ResourceValidationResult();

        public Task<ResourceValidationResult> ValidateRequestedResources(Client client, IEnumerable<string> requestedScopes, IEnumerable<string> requestedResourceIdentifiers)
        {
            return Task.FromResult(Result);
        }
    }
}
