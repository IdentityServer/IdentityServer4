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

        public Task<ResourceValidationResult> FilterResourcesAsync(ResourceValidationResult resourceValidationResult, IEnumerable<string> scopes)
        {
            var result = new ResourceValidationResult { 
                Resources = resourceValidationResult.Resources.Filter(scopes),
                Scopes = scopes.ToList()
            };
            return Task.FromResult(result);            
        }

        public Task<ResourceValidationResult> ValidateRequestedResourcesAsync(Client client, IEnumerable<string> requestedScopes, IEnumerable<string> requestedResourceIdentifiers)
        {
            return Task.FromResult(Result);
        }
    }
}
