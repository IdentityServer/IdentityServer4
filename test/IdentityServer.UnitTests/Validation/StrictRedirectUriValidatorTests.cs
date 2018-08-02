// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Xunit;

namespace IdentityServer.UnitTests.Validation
{
    public class StrictRedirectUriValidatorTests
    {
        StrictRedirectUriValidator _subject;
        IdentityServerOptions _options;
        Client _client;

        public StrictRedirectUriValidatorTests()
        {
            _client = new Client()
            {
                RedirectUris = { "http://callback", "custom://callback" },
                PostLogoutRedirectUris = { "http://post", "custom://post" }
            };

            _options = new IdentityServerOptions();
            _options.Validation.InvalidRedirectUriPrefixes.Clear();
            _subject = new StrictRedirectUriValidator(_options);
        }

        [Fact]
        public async Task IsRedirectUriValidAsync_for_invalid_scheme_should_return_false()
        {
            _options.Validation.InvalidRedirectUriPrefixes.Add("custom");
            var result = await _subject.IsRedirectUriValidAsync("custom://callback", _client);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRedirectUriValidAsync_for_valid_scheme_should_return_true()
        {
            var result = await _subject.IsRedirectUriValidAsync("custom://callback", _client);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsPostLogoutRedirectUriValidAsync_for_invalid_scheme_should_return_false()
        {
            _options.Validation.InvalidRedirectUriPrefixes.Add("custom");
            var result = await _subject.IsPostLogoutRedirectUriValidAsync("custom://post", _client);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsPostLogoutRedirectUriValidAsync_for_valid_scheme_should_return_true()
        {
            var result = await _subject.IsPostLogoutRedirectUriValidAsync("custom://post", _client);
            result.Should().BeTrue();
        }
    }
}
