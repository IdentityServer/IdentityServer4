// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Services.InMemory;
using IdentityServer4.Core.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Tests.Validation
{

    public class ScopeValidation
    {
        const string Category = "Scope Validation";

        List<Scope> _allScopes = new List<Scope>
            {
                new Scope
                {
                    Name = "openid",
                    Type = ScopeType.Identity,
                    Required = true
                },
                new Scope
                {
                    Name = "email",
                    Type = ScopeType.Identity
                },
                new Scope
                {
                    Name = "resource1",
                    Type = ScopeType.Resource,
                    Required = true
                },
                new Scope
                {
                    Name = "resource2",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "disabled",
                    Enabled = false,
                    Type = ScopeType.Resource
                },
            };

        Client _unrestrictedClient = new Client
            {
                ClientId = "unrestricted",
                AllowAccessToAllScopes = true
            };

        Client _restrictedClient = new Client
            {
                ClientId = "restricted",
            
                AllowedScopes = new List<string>
                {
                    "openid",
                    "resource1",
                    "disabled"
                }
            };

        IScopeStore _store;

        public ScopeValidation()
        {
            _store = new InMemoryScopeStore(_allScopes);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Parse_Scopes_with_Empty_Scope_List()
        {
            var scopes = "".ParseScopesString();

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
        public async Task All_Scopes_Valid()
        {
            var scopes = "openid email resource1 resource2".ParseScopesString();
            
            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_Scope()
        {
            var scopes = "openid email resource1 resource2 unknown".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Disabled_Scope()
        {
            var scopes = "openid email resource1 resource2 disabled".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Allowed_For_Unrestricted_Client()
        {
            var scopes = "openid email resource1 resource2".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = validator.AreScopesAllowed(_unrestrictedClient, scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void All_Scopes_Allowed_For_Restricted_Client()
        {
            var scopes = "openid resource1".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Restricted_Scopes()
        {
            var scopes = "openid email resource1 resource2".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = validator.AreScopesAllowed(_restrictedClient, scopes);

            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Resource_and_Identity_Scopes()
        {
            var scopes = "openid email resource1 resource2".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeTrue();
            validator.ContainsResourceScopes.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Resource_Scopes_Only()
        {
            var scopes = "resource1 resource2".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeFalse();
            validator.ContainsResourceScopes.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Contains_Identity_Scopes_Only()
        {
            var scopes = "openid email".ParseScopesString();

            var validator = Factory.CreateScopeValidator(_store);
            var result = await validator.AreScopesValidAsync(scopes);

            result.Should().BeTrue();
            validator.ContainsOpenIdScopes.Should().BeTrue();
            validator.ContainsResourceScopes.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public void ValidateRequiredScopes_required_scopes_present_should_succeed()
        {
            var validator = Factory.CreateScopeValidator(_store);
            validator.RequestedScopes.AddRange(_allScopes);
            validator.ValidateRequiredScopes(new string[] { "openid", "email", "resource1", "resource2" }).Should().BeTrue();
            validator.ValidateRequiredScopes(new string[] { "openid", "email", "resource1" }).Should().BeTrue();
            validator.ValidateRequiredScopes(new string[] { "openid", "resource1", "resource2" }).Should().BeTrue();
            validator.ValidateRequiredScopes(new string[] { "openid", "resource1" }).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public void ValidateRequiredScopes_required_scopes_absent_should_fail()
        {
            var validator = Factory.CreateScopeValidator(_store);
            validator.RequestedScopes.AddRange(_allScopes);
            validator.ValidateRequiredScopes(new string[] { "email", "resource2" }).Should().BeFalse();
            validator.ValidateRequiredScopes(new string[] { "email", "resource1", "resource2" }).Should().BeFalse();
            validator.ValidateRequiredScopes(new string[] { "openid", "resource2" }).Should().BeFalse();
        }
    }
}