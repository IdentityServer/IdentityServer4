// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.UnitTests.Validation;
using IdentityServer4.Validation;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer.UnitTests.Validation
{
    public class ClientConfigurationValidation
    {
        private const string Category = "Client Configuration Validation Tests";
        private IClientConfigurationValidator _validator;
        IdentityServerOptions _options;

        public ClientConfigurationValidation()
        {
            _options = new IdentityServerOptions();
            _validator = new DefaultClientConfigurationValidator(_options);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Standard_clients_should_succeed()
        {
            foreach (var client in TestClients.Get())
            {
                // deliberate invalid configuration
                if (client.ClientId == "implicit_and_client_creds") continue;

                var context = await ValidateAsync(client);

                if (!context.IsValid)
                {
                    throw new System.Exception($"client {client.ClientId} failed configuration validation: {context.ErrorMessage}");
                }

            }
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_access_token_lifetime_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequireClientSecret = false,
                AllowedScopes = { "foo" },

                AccessTokenLifetime = 0
            };

            await ShouldFailAsync(client, "access token lifetime is 0 or negative");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_identity_token_lifetime_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireClientSecret = false,
                AllowedScopes = { "foo" },

                IdentityTokenLifetime = 0
            };

            await ShouldFailAsync(client, "identity token lifetime is 0 or negative");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_absolute_refresh_token_lifetime_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireClientSecret = false,
                AllowedScopes = { "foo" },

                AbsoluteRefreshTokenLifetime = -1
            };

            await ShouldFailAsync(client, "absolute refresh token lifetime is negative");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_sliding_refresh_token_lifetime_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireClientSecret = false,
                AllowedScopes = { "foo" },

                SlidingRefreshTokenLifetime = -1
            };

            await ShouldFailAsync(client, "sliding refresh token lifetime is negative");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_allowed_grant_type_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                RequireClientSecret = false,
                AllowedScopes = { "foo" },
            };

            await ShouldFailAsync(client, "no allowed grant type specified");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_client_secret_for_client_credentials_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "foo" },
            };

            await ShouldFailAsync(client, "Client secret is required for client_credentials, but no client secret is configured.");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_client_secret_for_implicit_and_client_credentials_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                RedirectUris = { "https://foo" },
                AllowedScopes = { "foo" },
            };

            await ShouldFailAsync(client, "Client secret is required for client_credentials, but no client secret is configured.");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_client_secret_for_hybrid_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.Hybrid,
                RedirectUris = { "https://foo" },
                AllowedScopes = { "foo" },
            };

            await ShouldFailAsync(client, "Client secret is required for hybrid, but no client secret is configured.");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_client_secret_for_code_should_fail()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "https://foo" },
                AllowedScopes = { "foo" },
            };

            await ShouldFailAsync(client, "Client secret is required for authorization_code, but no client secret is configured.");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Not_required_client_secret_for_hybrid_should_succeed()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.Hybrid,
                RequireClientSecret = false,
                RedirectUris = { "https://foo" },
                AllowedScopes = { "foo" },
            };

            var context = await ValidateAsync(client);
            context.IsValid.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_client_secret_for_implicit_should_succeed()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = { "foo" },
                RedirectUris = { "https://foo" }
            };

            var context = await ValidateAsync(client);
            context.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateUriSchemesAsync_for_invalid_redirecturi_scheme_should_fail()
        {
            _options.Validation.InvalidRedirectUriPrefixes.Add("custom");
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = { "foo" },
                RedirectUris = { "http://callback", "custom://callback" }
            };

            var result = await ValidateAsync(client);
            await ShouldFailAsync(client, "RedirectUri 'custom://callback' uses invalid scheme. If this scheme should be allowed, then configure it via ValidationOptions.");
        }

        [Fact]
        public async Task ValidateUriSchemesAsync_for_valid_redirect_uri_scheme_should_succeed()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = { "foo" },
                RedirectUris = { "http://callback", "custom://callback" }
            };

            var result = await ValidateAsync(client);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateUriSchemesAsync_for_invalid_post_logout_redirect_uri_scheme_should_fail()
        {
            _options.Validation.InvalidRedirectUriPrefixes.Add("custom");
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = { "foo" },
                RedirectUris = { "http://callback" },
                PostLogoutRedirectUris = { "http://postcallback", "custom://postcallback" }
            };

            var result = await ValidateAsync(client);
            await ShouldFailAsync(client, "PostLogoutRedirectUri 'custom://postcallback' uses invalid scheme. If this scheme should be allowed, then configure it via ValidationOptions.");
        }

        [Fact]
        public async Task ValidateUriSchemesAsync_for_valid_post_logout_redirect_uri_scheme_should_succeed()
        {
            var client = new Client
            {
                ClientId = "id",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = { "foo" },
                RedirectUris = { "http://callback" },
                PostLogoutRedirectUris = { "http://postcallback", "custom://postcallback" }
            };

            var result = await ValidateAsync(client);
            result.IsValid.Should().BeTrue();
        }

        private async Task<ClientConfigurationValidationContext> ValidateAsync(Client client)
        {
            var context = new ClientConfigurationValidationContext(client);
            await _validator.ValidateAsync(context);

            return context;
        }

        private async Task ShouldFailAsync(Client client, string expectedError)
        {
            var context = await ValidateAsync(client);

            context.IsValid.Should().BeFalse();
            context.ErrorMessage.Should().Be(expectedError);
        }
    }
}