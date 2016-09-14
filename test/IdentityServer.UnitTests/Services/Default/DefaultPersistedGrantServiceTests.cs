// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services.Default;
using IdentityServer4.Stores.InMemory;
using IdentityServer4.UnitTests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Services.Default
{
    public class DefaultPersistedGrantServiceTests
    {
        DefaultPersistedGrantService _subject;
        InMemoryPersistedGrantStore _grantStore = new InMemoryPersistedGrantStore();
        ClaimsPrincipal _user = IdentityServerPrincipal.Create("123", "bob");

        public DefaultPersistedGrantServiceTests()
        {
            _subject = new DefaultPersistedGrantService(
                _grantStore, 
                new Stores.Serialization.PersistentGrantSerializer(), 
                TestLogger.Create<DefaultPersistedGrantService>());
        }

        [Fact]
        public async Task StoreAuthorizationCodeAsync_should_persist_grant()
        {
            var code1 = new AuthorizationCode()
            {
                ClientId = "test",
                CreationTime = DateTime.Now,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "scope1", "scope2" }
            };

            await _subject.StoreAuthorizationCodeAsync("key", code1);
            var code2 = await _subject.GetAuthorizationCodeAsync("key");

            code1.ClientId.Should().Be(code2.ClientId);
            code1.CreationTime.Should().Be(code2.CreationTime);
            code1.Lifetime.Should().Be(code2.Lifetime);
            code1.Subject.GetSubjectId().Should().Be(code2.Subject.GetSubjectId());
            code1.CodeChallenge.Should().Be(code2.CodeChallenge);
            code1.RedirectUri.Should().Be(code2.RedirectUri);
            code1.Nonce.Should().Be(code2.Nonce);
            code1.RequestedScopes.ShouldBeEquivalentTo(code2.RequestedScopes);
        }

        [Fact]
        public async Task RemoveAuthorizationCodeAsync_should_remove_grant()
        {
            var code1 = new AuthorizationCode()
            {
                ClientId = "test",
                CreationTime = DateTime.Now,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "scope1", "scope2" }
            };

            await _subject.StoreAuthorizationCodeAsync("key", code1);
            await _subject.RemoveAuthorizationCodeAsync("key");
            var code2 = await _subject.GetAuthorizationCodeAsync("key");
            code2.Should().BeNull();
        }

        [Fact]
        public async Task StoreRefreshTokenAsync_should_persist_grant()
        {
            var token1 = new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token {
                    ClientId = "client",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "123"),
                        new Claim("scope", "foo")
                    }
                },
                Version = 1
            };

            await _subject.StoreRefreshTokenAsync("key", token1);
            var token2 = await _subject.GetRefreshTokenAsync("key");

            token1.ClientId.Should().Be(token2.ClientId);
            token1.CreationTime.Should().Be(token2.CreationTime);
            token1.Lifetime.Should().Be(token2.Lifetime);
            token1.Subject.GetSubjectId().Should().Be(token2.Subject.GetSubjectId());
            token1.Version.Should().Be(token2.Version);
            token1.AccessToken.Audience.Should().Be(token2.AccessToken.Audience);
            token1.AccessToken.ClientId.Should().Be(token2.AccessToken.ClientId);
            token1.AccessToken.CreationTime.Should().Be(token2.AccessToken.CreationTime);
            token1.AccessToken.Type.Should().Be(token2.AccessToken.Type);
        }

        [Fact]
        public async Task RemoveRefreshTokenAsync_should_remove_grant()
        {
            var token1 = new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = "client",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "123"),
                        new Claim("scope", "foo")
                    }
                },
                Version = 1
            };


            await _subject.StoreRefreshTokenAsync("key", token1);
            await _subject.RemoveRefreshTokenAsync("key");
            var token2 = await _subject.GetRefreshTokenAsync("key");
            token2.Should().BeNull();
        }

        [Fact]
        public async Task RemoveRefreshTokenAsync_by_sub_and_client_should_remove_grant()
        {
            var token1 = new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = "client",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "123"),
                        new Claim("scope", "foo")
                    }
                },
                Version = 1
            };

            await _subject.StoreRefreshTokenAsync("key1", token1);
            await _subject.StoreRefreshTokenAsync("key2", token1);
            await _subject.RemoveRefreshTokensAsync("123", "client");

            var token2 = await _subject.GetRefreshTokenAsync("key1");
            token2.Should().BeNull();
            token2 = await _subject.GetRefreshTokenAsync("key2");
            token2.Should().BeNull();
        }

        [Fact]
        public async Task StoreReferenceTokenAsync_should_persist_grant()
        {
            var token1 = new Token()
            {
                ClientId = "client",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "foo")
                },
                Version = 1
            };

            await _subject.StoreReferenceTokenAsync("key", token1);
            var token2 = await _subject.GetReferenceTokenAsync("key");

            token1.ClientId.Should().Be(token2.ClientId);
            token1.Audience.Should().Be(token2.Audience);
            token1.CreationTime.Should().Be(token2.CreationTime);
            token1.Type.Should().Be(token2.Type);
            token1.Lifetime.Should().Be(token2.Lifetime);
            token1.Version.Should().Be(token2.Version);
        }

        [Fact]
        public async Task RemoveReferenceTokenAsync_should_remove_grant()
        {
            var token1 = new Token()
            {
                ClientId = "client",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "foo")
                },
                Version = 1
            };

            await _subject.StoreReferenceTokenAsync("key", token1);
            await _subject.RemoveReferenceTokenAsync("key");
            var token2 = await _subject.GetReferenceTokenAsync("key");
            token2.Should().BeNull();
        }

        [Fact]
        public async Task RemoveReferenceTokenAsync_by_sub_and_client_should_remove_grant()
        {
            var token1 = new Token()
            {
                ClientId = "client",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "foo")
                },
                Version = 1
            };

            await _subject.StoreReferenceTokenAsync("key1", token1);
            await _subject.StoreReferenceTokenAsync("key2", token1);
            await _subject.RemoveReferenceTokensAsync("123", "client");

            var token2 = await _subject.GetReferenceTokenAsync("key1");
            token2.Should().BeNull();
            token2 = await _subject.GetReferenceTokenAsync("key2");
            token2.Should().BeNull();
        }

        [Fact]
        public async Task StoreUserConsentAsync_should_persist_grant()
        {
            var consent1 = new Consent()
            {
                ClientId = "client",
                SubjectId = "123",
                Scopes = new string[] { "foo", "bar" }
            };

            await _subject.StoreUserConsentAsync(consent1);
            var consent2 = await _subject.GetUserConsentAsync("123", "client");

            consent2.ClientId.Should().Be(consent1.ClientId);
            consent2.SubjectId.Should().Be(consent1.SubjectId);
            consent2.Scopes.ShouldBeEquivalentTo(new string[] { "bar", "foo" });
        }

        [Fact]
        public async Task RemoveUserConsentAsync_should_remove_grant()
        {
            var consent1 = new Consent()
            {
                ClientId = "client",
                SubjectId = "123",
                Scopes = new string[] { "foo", "bar" }
            };

            await _subject.StoreUserConsentAsync(consent1);
            await _subject.RemoveUserConsentAsync("123", "client");
            var consent2 = await _subject.GetUserConsentAsync("123", "client");
            consent2.Should().BeNull();
        }

        [Fact]
        public async Task same_key_for_different_grant_types_should_not_interfere_with_each_other()
        {
            await _subject.StoreReferenceTokenAsync("key", new Token()
            {
                ClientId = "client1",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Lifetime = 1,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar1"),
                    new Claim("scope", "bar2"),
                },
            });
            await _subject.StoreRefreshTokenAsync("key", new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 2,
                AccessToken = new Token
                {
                    ClientId = "client1",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "123"),
                        new Claim("scope", "baz1"),
                        new Claim("scope", "baz2")
                    }
                },
                Version = 1
            });
            await _subject.StoreAuthorizationCodeAsync("key", new AuthorizationCode()
            {
                ClientId = "client1",
                CreationTime = DateTime.Now,
                Lifetime = 3,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux1", "quux2" }
            });

            (await _subject.GetAuthorizationCodeAsync("key")).Lifetime.Should().Be(3);
            (await _subject.GetRefreshTokenAsync("key")).Lifetime.Should().Be(2);
            (await _subject.GetReferenceTokenAsync("key")).Lifetime.Should().Be(1);
        }

        [Fact]
        public async Task GetAllGrantsAsync_should_return_all_grants()
        {
            await _subject.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client1",
                SubjectId = "123",
                Scopes = new string[] { "foo1", "foo2" }
            });
            await _subject.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client2",
                SubjectId = "123",
                Scopes = new string[] { "foo3" }
            });
            await _subject.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client1",
                SubjectId = "456",
                Scopes = new string[] { "foo3" }
            });

            await _subject.StoreReferenceTokenAsync("key1", new Token()
            {
                ClientId = "client1",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar1"),
                    new Claim("scope", "bar2"),
                },
            });
            await _subject.StoreReferenceTokenAsync("key2", new Token()
            {
                ClientId = "client2",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar3"),
                },
            });
            await _subject.StoreReferenceTokenAsync("key3", new Token()
            {
                ClientId = "client1",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "456"),
                    new Claim("scope", "bar3"),
                },
            });

            await _subject.StoreRefreshTokenAsync("key4", new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = "client1",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "123"),
                        new Claim("scope", "baz1"),
                        new Claim("scope", "baz2")
                    }
                },
                Version = 1
            });
            await _subject.StoreRefreshTokenAsync("key5", new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = "client1",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "456"),
                        new Claim("scope", "baz3"),
                    }
                },
                Version = 1
            });
            await _subject.StoreRefreshTokenAsync("key6", new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = "client2",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "123"),
                        new Claim("scope", "baz3"),
                    }
                },
                Version = 1
            });

            await _subject.StoreAuthorizationCodeAsync("key7", new AuthorizationCode()
            {
                ClientId = "client1",
                CreationTime = DateTime.Now,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux1", "quux2" }
            });
            await _subject.StoreAuthorizationCodeAsync("key8", new AuthorizationCode()
            {
                ClientId = "client2",
                CreationTime = DateTime.Now,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });
            
            await _subject.StoreAuthorizationCodeAsync("key9", new AuthorizationCode()
            {
                ClientId = "client1",
                CreationTime = DateTime.Now,
                Lifetime = 10,
                Subject = IdentityServerPrincipal.Create("456", "alice"),
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });

            var grants = await _subject.GetAllGrantsAsync("123");

            grants.Count().Should().Be(2);
            var grant1 = grants.First(x => x.ClientId == "client1");
            grant1.SubjectId.Should().Be("123");
            grant1.ClientId.Should().Be("client1");
            grant1.Scopes.ShouldBeEquivalentTo(new string[] { "foo1", "foo2", "bar1", "bar2", "baz1", "baz2", "quux1", "quux2" });

            var grant2 = grants.First(x => x.ClientId == "client2");
            grant2.SubjectId.Should().Be("123");
            grant2.ClientId.Should().Be("client2");
            grant2.Scopes.ShouldBeEquivalentTo(new string[] { "foo3", "bar3", "baz3", "quux3" });
        }

        [Fact]
        public async Task RemoveAllGrantsAsync_should_remove_all_grants()
        {
            await _subject.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client1",
                SubjectId = "123",
                Scopes = new string[] { "foo1", "foo2" }
            });
            await _subject.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client2",
                SubjectId = "123",
                Scopes = new string[] { "foo3" }
            });
            await _subject.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client1",
                SubjectId = "456",
                Scopes = new string[] { "foo3" }
            });

            await _subject.StoreReferenceTokenAsync("key1", new Token()
            {
                ClientId = "client1",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar1"),
                    new Claim("scope", "bar2"),
                },
            });
            await _subject.StoreReferenceTokenAsync("key2", new Token()
            {
                ClientId = "client2",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "123"),
                    new Claim("scope", "bar3"),
                },
            });
            await _subject.StoreReferenceTokenAsync("key3", new Token()
            {
                ClientId = "client1",
                Audience = "aud",
                CreationTime = DateTime.Now,
                Type = "type",
                Claims = new List<Claim>
                {
                    new Claim("sub", "456"),
                    new Claim("scope", "bar3"),
                },
            });

            await _subject.StoreRefreshTokenAsync("key4", new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = "client1",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "123"),
                        new Claim("scope", "baz1"),
                        new Claim("scope", "baz2")
                    }
                },
                Version = 1
            });
            await _subject.StoreRefreshTokenAsync("key5", new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = "client1",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "456"),
                        new Claim("scope", "baz3"),
                    }
                },
                Version = 1
            });
            await _subject.StoreRefreshTokenAsync("key6", new RefreshToken()
            {
                CreationTime = DateTime.Now,
                Lifetime = 10,
                AccessToken = new Token
                {
                    ClientId = "client2",
                    Audience = "aud",
                    CreationTime = DateTime.Now,
                    Type = "type",
                    Claims = new List<Claim>
                    {
                        new Claim("sub", "123"),
                        new Claim("scope", "baz3"),
                    }
                },
                Version = 1
            });

            await _subject.StoreAuthorizationCodeAsync("key7", new AuthorizationCode()
            {
                ClientId = "client1",
                CreationTime = DateTime.Now,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux1", "quux2" }
            });
            await _subject.StoreAuthorizationCodeAsync("key8", new AuthorizationCode()
            {
                ClientId = "client2",
                CreationTime = DateTime.Now,
                Lifetime = 10,
                Subject = _user,
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });

            await _subject.StoreAuthorizationCodeAsync("key9", new AuthorizationCode()
            {
                ClientId = "client1",
                CreationTime = DateTime.Now,
                Lifetime = 10,
                Subject = IdentityServerPrincipal.Create("456", "alice"),
                CodeChallenge = "challenge",
                RedirectUri = "http://client/cb",
                Nonce = "nonce",
                RequestedScopes = new string[] { "quux3" }
            });

            await _subject.RemoveAllGrantsAsync("123", "client1");

            (await _subject.GetReferenceTokenAsync("key1")).Should().BeNull();
            (await _subject.GetReferenceTokenAsync("key2")).Should().NotBeNull();
            (await _subject.GetReferenceTokenAsync("key3")).Should().NotBeNull();
            (await _subject.GetRefreshTokenAsync("key4")).Should().BeNull();
            (await _subject.GetRefreshTokenAsync("key5")).Should().NotBeNull();
            (await _subject.GetRefreshTokenAsync("key6")).Should().NotBeNull();
            (await _subject.GetAuthorizationCodeAsync("key7")).Should().BeNull();
            (await _subject.GetAuthorizationCodeAsync("key8")).Should().NotBeNull();
            (await _subject.GetAuthorizationCodeAsync("key9")).Should().NotBeNull();
        }
    }
}
