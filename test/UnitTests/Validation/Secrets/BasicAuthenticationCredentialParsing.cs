// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using FluentAssertions;
using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Validation;
using System;
using System.Text;
using Xunit;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Tests.Validation.Secrets
{
    public class BasicAuthenticationSecretParsing
    {
        const string Category = "Secrets - Basic Authentication Secret Parsing";
        IdentityServerOptions _options;
        BasicAuthenticationSecretParser _parser;

        public BasicAuthenticationSecretParsing()
        {
            _options = new IdentityServerOptions();
            _parser = new BasicAuthenticationSecretParser(_options, new LoggerFactory());
        }

        [Fact]
        public async void EmptyContext()
        {
            var context = new DefaultHttpContext();

            var secret = await _parser.ParseAsync(context);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request()
        {
            var context = new DefaultHttpContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:secret")));
            context.Request.Headers.Add("Authorization", new StringValues(headerValue));


            var secret = await _parser.ParseAsync(context);

            secret.Type.Should().Be(Constants.ParsedSecretTypes.SharedSecret);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("secret");
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header()
        {
            var context = new DefaultHttpContext();

            context.Request.Headers.Add("Authorization", new StringValues(""));
                
            var secret = await _parser.ParseAsync(context);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request_ClientId_Too_Long()
        {
            var context = new DefaultHttpContext();

            var longClientId = "x".Repeat(_options.InputLengthRestrictions.ClientId + 1);
            var credential = string.Format("{0}:secret", longClientId);

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(credential)));
            context.Request.Headers.Add("Authorization", new StringValues(headerValue));

            var secret = await _parser.ParseAsync(context);
            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void Valid_BasicAuthentication_Request_ClientSecret_Too_Long()
        {
            var context = new DefaultHttpContext();

            var longClientSecret = "x".Repeat(_options.InputLengthRestrictions.ClientSecret + 1);
            var credential = string.Format("client:{0}", longClientSecret);

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(credential)));
            context.Request.Headers.Add("Authorization", new StringValues(headerValue));

            var secret = await _parser.ParseAsync(context);
            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Empty_Basic_Header_Variation()
        {
            var context = new DefaultHttpContext();

            context.Request.Headers.Add("Authorization", new StringValues("Basic "));

            var secret = await _parser.ParseAsync(context);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Unknown_Scheme()
        {
            var context = new DefaultHttpContext();

            context.Request.Headers.Add("Authorization", new StringValues("Unknown"));

            var secret = await _parser.ParseAsync(context);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_NoBase64_Encoding()
        {
            var context = new DefaultHttpContext();

            context.Request.Headers.Add("Authorization", new StringValues("Basic somerandomdata"));

            var secret = await _parser.ParseAsync(context);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only()
        {
            var context = new DefaultHttpContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client")));
            context.Request.Headers.Add("Authorization", new StringValues(headerValue));

            var secret = await _parser.ParseAsync(context);

            secret.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async void BasicAuthentication_Request_With_Malformed_Credentials_Base64_Encoding_UserName_Only_With_Colon()
        {
            var context = new DefaultHttpContext();

            var headerValue = string.Format("Basic {0}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("client:")));
            context.Request.Headers.Add("Authorization", new StringValues(headerValue));

            var secret = await _parser.ParseAsync(context);

            secret.Should().BeNull();
        }
    }
}