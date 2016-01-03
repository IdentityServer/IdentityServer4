// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNet.TestHost;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Threading;
using System;
using IdentityServer4.Core.Extensions;
using IdentityModel.Client;
using IdentityServer4.Tests.Common;
using IdentityServer4.Core;
using System.Collections.Generic;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services.InMemory;
using System.Security.Claims;
using System.Linq;

namespace IdentityServer4.Tests.Common
{
    public class AuthorizeEndpointTestBase
    {
        public const string DiscoveryEndpoint = "https://server/.well-known/openid-configuration";
        public const string LoginEndpoint = "https://server/ui/login";
        public const string AuthorizeEndpoint = "https://server/connect/authorize";

        protected readonly HttpClient _client;
        protected readonly Browser _browser;
        protected readonly HttpMessageHandler _handler;
        protected readonly MockAuthorizationPipeline _mockPipeline;
        protected readonly AuthorizeRequest _authorizeRequest = new AuthorizeRequest(AuthorizeEndpoint);

        public AuthorizeEndpointTestBase()
        {
            _mockPipeline = new MockAuthorizationPipeline(GetClients(), GetScopes(), GetUsers());
            var server = TestServer.Create(null, _mockPipeline.Configure, _mockPipeline.ConfigureServices);
            _handler = server.CreateHandler();
            _browser = new Browser(_handler);
            _client = new HttpClient(_browser);
        }

        public virtual IEnumerable<Client> GetClients()
        {
            return new List<Client>();
        }
        public virtual IEnumerable<Scope> GetScopes()
        {
            return new List<Scope>();
        }
        public virtual List<InMemoryUser> GetUsers()
        {
            return new List<InMemoryUser>();
        }

        public async Task LoginAsync(ClaimsPrincipal subject)
        {
            var old = _browser.AllowAutoRedirect;
            _browser.AllowAutoRedirect = false;

            _mockPipeline.Subject = subject;
            await _client.GetAsync(LoginEndpoint);

            _browser.AllowAutoRedirect = old;
        }

        public async Task LoginAsync(string subject)
        {
            var user = GetUsers().Single(x => x.Subject == subject);
            var name = user.Claims.Where(x => x.Type == "name").Select(x=>x.Value).FirstOrDefault() ?? user.Username;
            await LoginAsync(IdentityServerPrincipal.Create(subject, name));
        }

        public string CreateAuthorizeUrl(
            string clientId, 
            string responseType, 
            string scope = null, 
            string redirectUri = null, 
            string state = null, 
            string nonce = null, 
            string loginHint = null, 
            string acrValues = null, 
            string responseMode = null, 
            object extra = null)
        {
            var url = _authorizeRequest.CreateAuthorizeUrl(
                clientId: clientId,
                responseType: responseType,
                scope: scope,
                redirectUri: redirectUri,
                state: state,
                nonce: nonce,
                loginHint: loginHint,
                acrValues: acrValues,
                responseMode: responseMode, 
                extra: extra);
            return url;
        }
    }
}
