using FluentAssertions;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace IdentityServer.UnitTests.Validation
{
    public class GrantTypesValidation
    {
        const string Category = "Grant Types Validation";

        [Fact]
        [Trait("Category", Category)]
        public void implicit_should_be_allowed()
        {
            var client = new Client();
            client.AllowedGrantTypes = GrantTypes.Implicit;
        }

        [Fact]
        [Trait("Category", Category)]
        public void custom_should_be_allowed()
        {
            var client = new Client();
            client.AllowedGrantTypes = GrantTypes.List("custom");
        }

        [Fact]
        [Trait("Category", Category)]
        public void custom_should_be_allowed_raw()
        {
            var client = new Client();
            client.AllowedGrantTypes = new List<string> { "custom" };
        }
        
        [Theory]
        [Trait("Category", Category)]
        [InlineData(GrantType.Implicit, GrantType.Hybrid)]
        [InlineData(GrantType.Implicit, GrantType.AuthorizationCode)]
        [InlineData(GrantType.AuthorizationCode, GrantType.Hybrid)]
        public void forbidden_grant_type_combinations_should_throw(string type1, string type2)
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = GrantTypes.List(type1, type2);

            act.ShouldThrow<InvalidOperationException>();            
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData(GrantType.Implicit, GrantType.Hybrid)]
        [InlineData(GrantType.Implicit, GrantType.AuthorizationCode)]
        [InlineData(GrantType.AuthorizationCode, GrantType.Hybrid)]
        public void custom_and_forbidden_grant_type_combinations_should_throw(string type1, string type2)
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = GrantTypes.List("custom1", type2, "custom2", type1);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void duplicate_values_should_throw()
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = GrantTypes.List("custom1", "custom2", "custom1");

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void empty_grant_type_list_should_throw_single()
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = GrantTypes.List();

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void grant_type_with_space_should_throw_single()
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = GrantTypes.List("custo m2");

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void grant_type_with_space_should_throw_multiple()
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = GrantTypes.List("custom1", "custo m2", "custom1");

            act.ShouldThrow<InvalidOperationException>();
        }
    }
}
