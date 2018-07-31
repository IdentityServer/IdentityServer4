// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Models;
using Xunit;

namespace IdentityServer4.UnitTests.Validation.Secrets
{
    public class SecretHashCode
    {
        private const string Category = "Secrets - GetHashCode";

        [Fact]
        [Trait("Category", Category)]
        public void Null_Values()
        {
            var secret = new Secret(null);

            secret.GetHashCode().Should().NotBe(0);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Type_Null()
        {
            var secret = new Secret("value");

            secret.GetHashCode().Should().NotBe(0);
        }
    }
}
