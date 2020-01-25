using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

namespace IdentityServer.UnitTests.Stores
{
    public class InMemoryResourcesStoreTests
    {
        [Fact]
        public void InMemoryResourcesStore_should_throw_if_contains_duplicate_names()
        {
            List<IdentityResource> identityResources = new List<IdentityResource>
            {
                new IdentityResource { Name = "A" },
                new IdentityResource { Name = "A" },
                new IdentityResource { Name = "C" }
            };

            List<ApiResource> apiResources = new List<ApiResource>
            {
                new ApiResource { Name = "B" },
                new ApiResource { Name = "B" },
                new ApiResource { Name = "C" }
            };

            List<Scope> scopes = new List<Scope>
            {
                new Scope { Name = "B" },
                new Scope { Name = "C" },
                new Scope { Name = "C" },
            };

            Action act = () => new InMemoryResourcesStore(identityResources, null, null);
            act.Should().Throw<ArgumentException>();

            act = () => new InMemoryResourcesStore(null, apiResources, null);
            act.Should().Throw<ArgumentException>();
            
            act = () => new InMemoryResourcesStore(null, null, scopes);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void InMemoryResourcesStore_should_not_throw_if_does_not_contains_duplicate_names()
        {
            List<IdentityResource> identityResources = new List<IdentityResource>
            {
                new IdentityResource { Name = "A" },
                new IdentityResource { Name = "B" },
                new IdentityResource { Name = "C" }
            };

            List<ApiResource> apiResources = new List<ApiResource>
            {
                new ApiResource { Name = "A" },
                new ApiResource { Name = "B" },
                new ApiResource { Name = "C" }
            };

            List<Scope> scopes = new List<Scope>
            {
                new Scope { Name = "A" },
                new Scope { Name = "B" },
                new Scope { Name = "C" },
            };
            
            new InMemoryResourcesStore(identityResources, null, null);
            new InMemoryResourcesStore(null, apiResources, null);
            new InMemoryResourcesStore(null, null, scopes);
        }
    }
}
