﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using FluentAssertions;
using IdentityServer4.Models;
using Xunit;

namespace IdentityServer.UnitTests.Validation
{
    public class GrantTypesValidation
    {
        private const string Category = "Grant Types Validation";

        [Fact]
        [Trait("Category", Category)]
        public void empty_should_be_allowed()
        {
            var client = new Client();
            client.AllowedGrantTypes = new List<string>();
        }

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
            client.AllowedGrantTypes = new[] { "custom" };
        }

        [Fact]
        [Trait("Category", Category)]
        public void custom_should_be_allowed_raw()
        {
            var client = new Client();
            client.AllowedGrantTypes = new[] { "custom" };
        }
        
        [Theory]
        [Trait("Category", Category)]
        [InlineData(GrantType.Implicit, GrantType.Hybrid)]
        [InlineData(GrantType.Implicit, GrantType.AuthorizationCode)]
        [InlineData(GrantType.AuthorizationCode, GrantType.Hybrid)]
        public void forbidden_grant_type_combinations_should_throw(string type1, string type2)
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = new[] { type1, type2 };

            act.Should().Throw<InvalidOperationException>();            
        }

        [Theory]
        [Trait("Category", Category)]
        [InlineData(GrantType.Implicit, GrantType.Hybrid)]
        [InlineData(GrantType.Implicit, GrantType.AuthorizationCode)]
        [InlineData(GrantType.AuthorizationCode, GrantType.Hybrid)]
        public void custom_and_forbidden_grant_type_combinations_should_throw(string type1, string type2)
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = new[] { "custom1", type2, "custom2", type1 };

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void duplicate_values_should_throw()
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = new[] { "custom1", "custom2", "custom1" };

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void null_grant_type_list_should_throw_single()
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = null;

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void grant_type_with_space_should_throw_single()
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = new[] { "custo m2" };

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void grant_type_with_space_should_throw_multiple()
        {
            var client = new Client();

            Action act = () => client.AllowedGrantTypes = new[] { "custom1", "custo m2", "custom1" };

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void adding_invalid_value_to_collection_should_throw()
        {
            var client = new Client()
            {
                AllowedGrantTypes = { "implicit" }
            };

            Action act = () => client.AllowedGrantTypes.Add("authorization_code");

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void adding_valid_value_to_collection_should_succeed()
        {
            var client = new Client()
            {
                AllowedGrantTypes = { "implicit" }
            };

            client.AllowedGrantTypes.Add("custom");

            client.AllowedGrantTypes.Count.Should().Be(2);
        }
    }
}
