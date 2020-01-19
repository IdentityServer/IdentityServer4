// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Validation.Setup;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Xunit;

namespace IdentityServer.UnitTests.Validation
{
    public class ResourceValidation
    {
        private const string Category = "Resource Validation";

        private List<IdentityResource> _identityResources = new List<IdentityResource>
        {
            new IdentityResource
            {
                Name = "openid",
                Required = true
            },
            new IdentityResource
            {
                Name = "email"
            }
        };

        private List<ApiResource> _apiResources = new List<ApiResource>
        {
            new ApiResource
            {
                Name = "api",
                Scopes =
                {
                    new Scope
                    {
                        Name = "resource1",
                        Required = true
                    },
                    new Scope
                    {
                        Name = "resource2"
                    }
                }
            },
            new ApiResource
            {
                Name = "disabled_api",
                Enabled = false,
                Scopes =
                {
                    new Scope
                    {
                        Name = "disabled"
                    }
                }
            }
        };

        private Client _restrictedClient = new Client
        {
            ClientId = "restricted",

            AllowedScopes = new List<string>
                {
                    "openid",
                    "resource1",
                    "disabled"
                }
        };

        private IResourceStore _store;

        public ResourceValidation()
        {
            _store = new InMemoryResourcesStore(_identityResources, _apiResources);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Empty_Scope_List()
        {
            var scopes = string.Empty.ParseScopesString();

            scopes.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Sorting()
        {
            var scopes = "scope3 scope2 scope1".ParseScopesString();

            scopes.Count.Should().Be(3);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
            scopes[2].Should().Be("scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Extra_Spaces()
        {
            var scopes = "   scope3     scope2     scope1   ".ParseScopesString();

            scopes.Count.Should().Be(3);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
            scopes[2].Should().Be("scope3");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Duplicate_Scope()
        {
            var scopes = "scope2 scope1 scope2".ParseScopesString();

            scopes.Count.Should().Be(2);

            scopes[0].Should().Be("scope1");
            scopes[1].Should().Be("scope2");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Only_Offline_Access_Requested()
        {
            var scopes = "offline_access".ParseScopesString();

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

            result.Succeeded.Should().BeFalse();
            result.InvalidScopes.Should().BeEmpty();
            result.InvalidScopesForClient.Should().Contain("offline_access");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task All_Scopes_Valid()
        {
            var scopes = "openid resource1".ParseScopesString();

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

            result.Succeeded.Should().BeTrue();
            result.InvalidScopes.Should().BeEmpty();
            result.InvalidScopesForClient.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Scope()
        {
            {
                var scopes = "openid email resource1 unknown".ParseScopesString();

                var validator = Factory.CreateResourceValidator(_store);
                var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

                result.Succeeded.Should().BeFalse();
                result.InvalidScopes.Should().Contain("unknown");
                result.InvalidScopesForClient.Should().Contain("email");
            }
            {
                var scopes = "openid resource1 resource2".ParseScopesString();

                var validator = Factory.CreateResourceValidator(_store);
                var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

                result.Succeeded.Should().BeFalse();
                result.InvalidScopes.Should().BeEmpty();
                result.InvalidScopesForClient.Should().Contain("resource2");
            }
            {
                var scopes = "openid email resource1".ParseScopesString();

                var validator = Factory.CreateResourceValidator(_store);
                var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

                result.Succeeded.Should().BeFalse();
                result.InvalidScopes.Should().BeEmpty();
                result.InvalidScopesForClient.Should().Contain("email");
            }
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Disabled_Scope()
        {
            var scopes = "openid resource1 disabled".ParseScopesString();

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

            result.Succeeded.Should().BeFalse();
            result.InvalidScopes.Should().Contain("disabled");
            result.InvalidScopesForClient.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task All_Scopes_Allowed_For_Restricted_Client()
        {
            var scopes = "openid resource1".ParseScopesString();

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

            result.Succeeded.Should().BeTrue();
            result.InvalidScopes.Should().BeEmpty();
            result.InvalidScopesForClient.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Restricted_Scopes()
        {
            var scopes = "openid email resource1 resource2".ParseScopesString();

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

            result.Succeeded.Should().BeFalse();
            result.InvalidScopes.Should().BeEmpty();
            result.InvalidScopesForClient.Should().Contain("email");
            result.InvalidScopesForClient.Should().Contain("resource2");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Resource_and_Identity_Scopes()
        {
            var scopes = "openid resource1".ParseScopesString();

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

            result.Succeeded.Should().BeTrue();
            result.Resources.IdentityResources.SelectMany(x => x.Name).Should().Contain("openid");
            result.Resources.ApiResources.SelectMany(x => x.ToScopeNames()).Should().Contain("resource1");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Resource_Scopes_Only()
        {
            var scopes = "resource1".ParseScopesString();

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

            result.Succeeded.Should().BeTrue();
            result.Resources.IdentityResources.Should().BeEmpty();
            result.Resources.ApiResources.SelectMany(x => x.ToScopeNames()).Should().Contain("resource1");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Identity_Scopes_Only()
        {
            var scopes = "openid".ParseScopesString();

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(_restrictedClient, scopes, null);

            result.Succeeded.Should().BeTrue();
            result.Resources.IdentityResources.SelectMany(x => x.Name).Should().Contain("openid");
            result.Resources.ApiResources.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetRequiredScopeNames_should_return_correct_scopes()
        {
            var resources = new Resources(_identityResources, _apiResources);
            
            var result = resources.GetRequiredScopeNames();
            
            result.Should().BeEquivalentTo(new string[] { "openid", "resource1" });
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task Scope_matches_multipls_apis_should_succeed()
        {
            _apiResources.Clear();
            _apiResources.Add(new ApiResource { Name = "api1", Scopes = { new Scope { Name = "resource" } } });
            _apiResources.Add(new ApiResource { Name = "api2", Scopes = { new Scope { Name = "resource" } } });

            var validator = Factory.CreateResourceValidator(_store);
            var result = await validator.ValidateRequestedResourcesAsync(new Client { AllowedScopes = { "resource" } }, new[] { "resource" }, null);

            result.Succeeded.Should().BeTrue();
            result.Resources.ApiResources.Count.Should().Be(2);
            result.Resources.ApiResources.Select(x => x.Name).Should().BeEquivalentTo(new[] { "api1", "api2" });
            result.Scopes.Count().Should().Be(1);
            result.Scopes.Select(x => x.Name).Should().BeEquivalentTo(new[] { "resource" });
        }
    }
}