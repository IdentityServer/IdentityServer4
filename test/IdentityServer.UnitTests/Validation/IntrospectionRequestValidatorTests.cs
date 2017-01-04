// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Validation
{
    public class IntrospectionRequestValidatorTests
    {
        const string Category = "Introspection request validation";

        IntrospectionRequestValidator _subject;
        IReferenceTokenStore _referenceTokenStore;

        public IntrospectionRequestValidatorTests()
        {
            _referenceTokenStore = Factory.CreateReferenceTokenStore();
            var tokenValidator = Factory.CreateTokenValidator(_referenceTokenStore);

            _subject = new IntrospectionRequestValidator(tokenValidator, TestLogger.Create<IntrospectionRequestValidator>());
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task test()
        {
            var token = new Token {
                Issuer = "http://op",
                ClientId = "codeclient",
                Lifetime = 1000,
                Claims =
                {
                    new System.Security.Claims.Claim("scope", "a"),
                    new System.Security.Claims.Claim("scope", "b"),
                }
            };
            var handle = await _referenceTokenStore.StoreReferenceTokenAsync(token);
            
            var param = new NameValueCollection()
            {
                {"token", handle}
            };

            var api = new ApiResource("a");
            var result = await _subject.ValidateAsync(param, api);

            var scopes = result.Claims.Where(x => x.Type == "scope");
            scopes.Count().Should().Be(1);
            scopes.First().Value.Should().Be("a");
        }
    }
}
