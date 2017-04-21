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

namespace IdentityServer4.IntegrationTests.Endpoints.Authorize
{
    public class AuthorizeTests
    {
        const string Category = "Authorize endpoint";

        MockIdSvrUiPipeline _mockPipeline = new MockIdSvrUiPipeline();

        public AuthorizeTests()
        {
            _mockPipeline.Clients.AddRange(new Client[] {
                new Client
                {
                    ClientId = "client1",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid", "profile" },
                    RedirectUris = new List<string> { "https://client1/callback" },
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientId = "client2",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = true,
                    AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                    RedirectUris = new List<string> { "https://client2/callback" },
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientId = "client3",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
                    RedirectUris = new List<string> { "https://client3/callback" },
                    AllowAccessTokensViaBrowser = true,
                    IdentityProviderRestrictions = new List<string> { "google" }
                }
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

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_404()
        {
            var response = await _mockPipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.AuthorizeEndpoint);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task post_request_without_form_should_return_415()
        {
            var response = await _mockPipeline.BrowserClient.PostAsync(MockIdSvrUiPipeline.AuthorizeEndpoint, new StringContent("foo"));

            response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task post_request_should_return_200()
        {
            var response = await _mockPipeline.BrowserClient.PostAsync(MockIdSvrUiPipeline.AuthorizeEndpoint,
                new FormUrlEncodedContent(
                    new Dictionary<string, string>{ }));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task get_request_should_not_return_500()
        {
            var response = await _mockPipeline.BrowserClient.GetAsync(MockIdSvrUiPipeline.AuthorizeEndpoint);

            ((int)response.StatusCode).Should().BeLessThan(500);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task anonymous_user_should_be_redirected_to_login_page()
        {
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.LoginWasCalled.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signin_request_should_have_authorization_params()
        {
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce",
                loginHint:"login_hint_value",
                acrValues:"acr_1 acr_2 tenant:tenant_value idp:idp_value",
                extra: new {
                    display = "popup", // must use a valid value from the spec for display
                    ui_locales ="ui_locale_value",
                    custom_foo ="foo_value"
                });
            var response = await _mockPipeline.BrowserClient.GetAsync(url + "&foo=bar");

            _mockPipeline.LoginRequest.Should().NotBeNull();
            _mockPipeline.LoginRequest.ClientId.Should().Be("client1");
            _mockPipeline.LoginRequest.DisplayMode.Should().Be("popup");
            _mockPipeline.LoginRequest.UiLocales.Should().Be("ui_locale_value");
            _mockPipeline.LoginRequest.IdP.Should().Be("idp_value");
            _mockPipeline.LoginRequest.Tenant.Should().Be("tenant_value");
            _mockPipeline.LoginRequest.LoginHint.Should().Be("login_hint_value");
            _mockPipeline.LoginRequest.AcrValues.ShouldAllBeEquivalentTo(new string[] { "acr_2", "acr_1" });
            _mockPipeline.LoginRequest.Parameters.AllKeys.Should().Contain("foo");
            _mockPipeline.LoginRequest.Parameters["foo"].Should().Be("bar");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task signin_response_should_allow_successful_authorization_response()
        {
            _mockPipeline.Subject = IdentityServerPrincipal.Create("bob", "Bob Loblaw");
            _mockPipeline.BrowserClient.StopRedirectingAfter = 2;

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://client1/callback");

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IsError.Should().BeFalse();
            authorization.IdentityToken.Should().NotBeNull();
            authorization.State.Should().Be("123_state");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task authenticated_user_with_valid_request_should_receive_authorization_response()
        {
            await _mockPipeline.LoginAsync("bob");

            _mockPipeline.BrowserClient.AllowAutoRedirect = false;

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client1",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client1/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://client1/callback");

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IsError.Should().BeFalse();
            authorization.IdentityToken.Should().NotBeNull();
            authorization.State.Should().Be("123_state");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task client_requires_consent_should_show_consent_page()
        {
            await _mockPipeline.LoginAsync("bob");

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client2",
                responseType: "id_token",
                scope: "openid",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce"
            );
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ConsentWasCalled.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task consent_page_should_have_authorization_params()
        {
            await _mockPipeline.LoginAsync("bob");

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client2",
                responseType: "id_token token",
                scope: "openid api1 api2",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce",
                acrValues: "acr_1 acr_2 tenant:tenant_value",
                extra: new
                {
                    display = "popup", // must use a valid value form the spec for display
                    ui_locales = "ui_locale_value",
                    custom_foo = "foo_value"
                }
            );
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.ConsentRequest.Should().NotBeNull();
            _mockPipeline.ConsentRequest.ClientId.Should().Be("client2");
            _mockPipeline.ConsentRequest.DisplayMode.Should().Be("popup");
            _mockPipeline.ConsentRequest.UiLocales.Should().Be("ui_locale_value");
            _mockPipeline.ConsentRequest.ScopesRequested.ShouldAllBeEquivalentTo(new string[] { "api2", "openid", "api1" });
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task consent_response_should_allow_successful_authorization_response()
        {
            await _mockPipeline.LoginAsync("bob");

            _mockPipeline.ConsentResponse = new ConsentResponse()
            {
                ScopesConsented = new string[] { "openid", "api2" }
            };
            _mockPipeline.BrowserClient.StopRedirectingAfter = 2;

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client2",
                responseType: "id_token token",
                scope: "openid profile api1 api2",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://client2/callback");

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IsError.Should().BeFalse();
            authorization.IdentityToken.Should().NotBeNull();
            authorization.State.Should().Be("123_state");
            var scopes = authorization.Scope.Split(' ');
            scopes.ShouldAllBeEquivalentTo(new string[] { "api2", "openid" });
        }

        [Fact()]
        [Trait("Category", Category)]
        public async Task consent_response_missing_required_scopes_should_error()
        {
            await _mockPipeline.LoginAsync("bob");

            _mockPipeline.ConsentResponse = new ConsentResponse()
            {
                ScopesConsented = new string[] { "api2" }
            };
            _mockPipeline.BrowserClient.StopRedirectingAfter = 2;

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client2",
                responseType: "id_token token",
                scope: "openid profile api1 api2",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://client2/callback");

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IsError.Should().BeTrue();
            authorization.Error.Should().Be("access_denied");
            authorization.State.Should().Be("123_state");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task login_response_and_consent_response_should_receive_authorization_response()
        {
            _mockPipeline.Subject = IdentityServerPrincipal.Create("bob", "Bob Loblaw");

            _mockPipeline.ConsentResponse = new ConsentResponse()
            {
                ScopesConsented = new string[] { "openid", "api1", "profile" }
            };

            _mockPipeline.BrowserClient.StopRedirectingAfter = 4;

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client2",
                responseType: "id_token token",
                scope: "openid profile api1 api2",
                redirectUri: "https://client2/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("https://client2/callback");

            var authorization = new IdentityModel.Client.AuthorizeResponse(response.Headers.Location.ToString());
            authorization.IsError.Should().BeFalse();
            authorization.IdentityToken.Should().NotBeNull();
            authorization.State.Should().Be("123_state");
            var scopes = authorization.Scope.Split(' ');
            scopes.ShouldAllBeEquivalentTo(new string[] { "profile", "api1", "openid" });
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task idp_should_be_passed_to_login_page()
        {
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client3",
                responseType: "id_token",
                scope: "openid profile",
                redirectUri: "https://client3/callback",
                state: "123_state",
                nonce: "123_nonce", 
                acrValues: "idp:google");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.LoginWasCalled.Should().BeTrue();
            _mockPipeline.LoginRequest.IdP.Should().Be("google");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task idp_not_allowed_by_client_should_not_be_passed_to_login_page()
        {
            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client3",
                responseType: "id_token",
                scope: "openid profile",
                redirectUri: "https://client3/callback",
                state: "123_state",
                nonce: "123_nonce",
                acrValues: "idp:facebook");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.LoginWasCalled.Should().BeTrue();
            _mockPipeline.LoginRequest.IdP.Should().BeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task user_idp_not_allowed_by_client_should_cause_login_page()
        {
            await _mockPipeline.LoginAsync("bob");

            var url = _mockPipeline.CreateAuthorizeUrl(
                clientId: "client3",
                responseType: "id_token",
                scope: "openid profile",
                redirectUri: "https://client3/callback",
                state: "123_state",
                nonce: "123_nonce");
            var response = await _mockPipeline.BrowserClient.GetAsync(url);

            _mockPipeline.LoginWasCalled.Should().BeTrue();
            _mockPipeline.LoginRequest.IdP.Should().BeNull();
        }
    }
}
