// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Collections.Generic;
using IdentityServer4.Models;
using System.Security.Claims;
using IdentityServer4.IntegrationTests.Common;
using IdentityServer4.Test;
using System.Net.Http;
using IdentityModel;
using System;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.IntegrationTests.Endpoints.Authorize
{
    public class JwtRequestAuthorizeTests
    {
        private const string Category = "Authorize endpoint with JWT requests";

        private readonly IdentityServerPipeline _mockPipeline = new IdentityServerPipeline();
        private readonly Client _client;

        private string _symmetricJwk = @"{ 'kty': 'oct', 'use': 'sig', 'kid': '1', 'k': 'nYA-IFt8xTsdBHe9hunvizcp3Dt7f6qGqudq18kZHNtvqEGjJ9Ud-9x3kbQ-LYfLHS3xM2MpFQFg1JzT_0U_F8DI40oby4TvBDGszP664UgA8_5GjB7Flnrlsap1NlitvNpgQX3lpyTvC2zVuQ-UVsXbBDAaSBUSlnw7SE4LM8Ye2WYZrdCCXL8yAX9vIR7vf77yvNTEcBCI6y4JlvZaqMB4YKVSfygs8XqGGCHjLpE5bvI-A4ESbAUX26cVFvCeDg9pR6HK7BmwPMlO96krgtKZcXEJtUELYPys6-rbwAIdmxJxKxpgRpt0FRv_9fm6YPwG7QivYBX-vRwaodL1TA', 'alg': 'HS256'}";

        public JwtRequestAuthorizeTests()
        {
            _mockPipeline.Clients.AddRange(new Client[] {
                _client = new Client
                {
                    ClientName = "Client with keys",
                    ClientId = "client",
                    Enabled = true,

                    RedirectUris = { "https://client/callback" },

                    ClientSecrets =
                    {
                        new Secret
                        {
                            Type = IdentityServerConstants.SecretTypes.X509CertificateBase64,
                            Value = Convert.ToBase64String(TestCert.Load().Export(X509ContentType.Cert))
                        },
                        new Secret
                        {
                            Type = IdentityServerConstants.SecretTypes.JsonWebKey,
                            Value = _symmetricJwk
                        }
                    },

                    AllowedGrantTypes = GrantTypes.Implicit,

                    AllowedScopes = new List<string>
                    {
                        "openid", "profile", "api1", "api2"
                    }
                },
            });

            _mockPipeline.Users.Add(new TestUser
            {
                SubjectId = "bob",
                Username = "bob",
                Claims = new Claim[]
                {
                    new Claim("name", "Bob Loblaw"),
                    new Claim("email", "bob@loblaw.com"),
                    new Claim("role", "Attorney")
                }
            });

            _mockPipeline.IdentityScopes.AddRange(new IdentityResource[] {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            });
            _mockPipeline.ApiScopes.AddRange(new ApiResource[] {
                new ApiResource
                {
                    Name = "api",
                    Scopes =
                    {
                        new Scope
                        {
                            Name = "api1"
                        },
                        new Scope
                        {
                            Name = "api2"
                        }
                    }
                }
            });

            _mockPipeline.Initialize();
        }

        string CreateRequestJwt(string issuer, string audience, SigningCredentials credential, Claim[] claims)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.OutboundClaimTypeMap.Clear();

            var token = handler.CreateJwtSecurityToken(
                issuer: issuer, 
                audience: audience, 
                signingCredentials: credential, 
                subject: Identity.Create("pwd", claims));

            return handler.WriteToken(token);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_accept_valid_JWT_request_object_parameters_using_X509_certificate()
        {
            var requestJwt = CreateRequestJwt(
                issuer: _client.ClientId,
                audience: IdentityServerPipeline.BaseUrl,
                credential: new X509SigningCredentials(TestCert.Load()),
                new[] {
                    new Claim("client_id", _client.ClientId),
                    new Claim("response_type", "id_token"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo"),
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: _client.ClientId,
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.LoginRequest.Should().NotBeNull();
            _mockPipeline.LoginRequest.ClientId.Should().Be(_client.ClientId);
            _mockPipeline.LoginRequest.DisplayMode.Should().Be("popup");
            _mockPipeline.LoginRequest.UiLocales.Should().Be("ui_locale_value");
            _mockPipeline.LoginRequest.IdP.Should().Be("idp_value");
            _mockPipeline.LoginRequest.Tenant.Should().Be("tenant_value");
            _mockPipeline.LoginRequest.LoginHint.Should().Be("login_hint_value");
            _mockPipeline.LoginRequest.AcrValues.Should().BeEquivalentTo(new string[] { "acr_2", "acr_1" });
            _mockPipeline.LoginRequest.Parameters.AllKeys.Should().Contain("foo");
            _mockPipeline.LoginRequest.Parameters["foo"].Should().Be("123foo");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_accept_valid_JWT_request_object_parameters_using_symmetric_jwk()
        {
            var requestJwt = CreateRequestJwt(
                issuer: _client.ClientId,
                audience: IdentityServerPipeline.BaseUrl,
                credential: new SigningCredentials(new Microsoft.IdentityModel.Tokens.JsonWebKey(_symmetricJwk), "HS256"),
                new[] {
                    new Claim("client_id", _client.ClientId),
                    new Claim("response_type", "id_token"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo"),
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: _client.ClientId,
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.LoginRequest.Should().NotBeNull();
            _mockPipeline.LoginRequest.ClientId.Should().Be(_client.ClientId);
            _mockPipeline.LoginRequest.DisplayMode.Should().Be("popup");
            _mockPipeline.LoginRequest.UiLocales.Should().Be("ui_locale_value");
            _mockPipeline.LoginRequest.IdP.Should().Be("idp_value");
            _mockPipeline.LoginRequest.Tenant.Should().Be("tenant_value");
            _mockPipeline.LoginRequest.LoginHint.Should().Be("login_hint_value");
            _mockPipeline.LoginRequest.AcrValues.Should().BeEquivalentTo(new string[] { "acr_2", "acr_1" });
            _mockPipeline.LoginRequest.Parameters.AllKeys.Should().Contain("foo");
            _mockPipeline.LoginRequest.Parameters["foo"].Should().Be("123foo");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_reject_jwt_request_without_client_id()
        {
            var requestJwt = CreateRequestJwt(
                issuer: _client.ClientId,
                audience: IdentityServerPipeline.BaseUrl,
                credential: new X509SigningCredentials(TestCert.Load()),
                new[] {
                    new Claim("response_type", "id_token"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo"),
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
            _mockPipeline.ErrorMessage.ErrorDescription.Should().Be("Invalid client_id");
            _mockPipeline.LoginRequest.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_reject_jwt_request_if_audience_is_incorrect()
        {
            var requestJwt = CreateRequestJwt(
                issuer: _client.ClientId,
                audience: "invalid",
                credential: new X509SigningCredentials(TestCert.Load()),
                new[] {
                    new Claim("client_id", _client.ClientId),
                    new Claim("response_type", "id_token"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo"),
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: _client.ClientId,
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });

            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
            _mockPipeline.ErrorMessage.ErrorDescription.Should().Be("Invalid JWT request");
            _mockPipeline.LoginRequest.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_reject_jwt_request_if_issuer_does_not_match_client_id()
        {
            var requestJwt = CreateRequestJwt(
                issuer: "invalid",
                audience: IdentityServerPipeline.BaseUrl,
                credential: new X509SigningCredentials(TestCert.Load()),
                new[] {
                    new Claim("client_id", _client.ClientId),
                    new Claim("response_type", "id_token"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo"),
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: _client.ClientId,
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });

            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
            _mockPipeline.ErrorMessage.ErrorDescription.Should().Be("Invalid JWT request");
            _mockPipeline.LoginRequest.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_reject_jwt_request_that_includes_request_param()
        {
            var requestJwt = CreateRequestJwt(
                issuer: _client.ClientId,
                audience: IdentityServerPipeline.BaseUrl,
                credential: new X509SigningCredentials(TestCert.Load()),
                new[] {
                    new Claim("response_type", "id_token"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo"),
                    new Claim("request", "request")
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: _client.ClientId,
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });

            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
            _mockPipeline.ErrorMessage.ErrorDescription.Should().Be("Invalid JWT request");
            _mockPipeline.LoginRequest.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_reject_jwt_request_that_includes_request_uri_param()
        {
            var requestJwt = CreateRequestJwt(
                issuer: _client.ClientId,
                audience: IdentityServerPipeline.BaseUrl,
                credential: new X509SigningCredentials(TestCert.Load()),
                new[] {
                    new Claim("response_type", "id_token"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo"),
                    new Claim("request_uri", "request_uri")
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: _client.ClientId,
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });

            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
            _mockPipeline.ErrorMessage.ErrorDescription.Should().Be("Invalid JWT request");
            _mockPipeline.LoginRequest.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_reject_jwt_request_if_response_type_does_not_match()
        {
            var requestJwt = CreateRequestJwt(
                issuer: _client.ClientId,
                audience: IdentityServerPipeline.BaseUrl,
                credential: new X509SigningCredentials(TestCert.Load()),
                new[] {
                    new Claim("response_type", "id_token token"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo")
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: _client.ClientId,
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });

            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
            _mockPipeline.ErrorMessage.ErrorDescription.Should().Be("Invalid JWT request");
            _mockPipeline.LoginRequest.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authorize_should_reject_jwt_request_if_client_id_does_not_match()
        {
            var requestJwt = CreateRequestJwt(
                issuer: _client.ClientId,
                audience: IdentityServerPipeline.BaseUrl,
                credential: new X509SigningCredentials(TestCert.Load()),
                new[] {
                    new Claim("response_type", "id_token"),
                    new Claim("client_id", "invalid"),
                    new Claim("scope", "openid profile"),
                    new Claim("state", "123state"),
                    new Claim("nonce", "123nonce"),
                    new Claim("redirect_uri", "https://client/callback"),
                    new Claim("acr_values", "acr_1 acr_2 tenant:tenant_value idp:idp_value"),
                    new Claim("login_hint", "login_hint_value"),
                    new Claim("display", "popup"),
                    new Claim("ui_locales", "ui_locale_value"),
                    new Claim("foo", "123foo")
            });

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: _client.ClientId,
                responseType: "id_token",
                extra: new
                {
                    request = requestJwt
                });

            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ErrorMessage.Error.Should().Be("invalid_request");
            _mockPipeline.ErrorMessage.ErrorDescription.Should().Be("Invalid JWT request");
            _mockPipeline.LoginRequest.Should().BeNull();
        }
    }
}