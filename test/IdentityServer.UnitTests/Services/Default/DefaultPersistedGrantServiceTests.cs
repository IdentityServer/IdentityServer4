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
        InMemoryPersistedGrantStore _store = new InMemoryPersistedGrantStore();
        ClaimsPrincipal _user = IdentityServerPrincipal.Create("123", "bob");

        public DefaultPersistedGrantServiceTests()
        {
            _subject = new DefaultPersistedGrantService(
                _store, 
                new Stores.Serialization.PersistentGrantSerializer(), 
                TestLogger.Create<DefaultPersistedGrantService>());
        }


        [Fact]
        public async Task GetAllGrantsAsync_should_return_all_grants()
        {
            await _store.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client1",
                SubjectId = "123",
                Scopes = new string[] { "foo1", "foo2" }
            });
            await _store.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client2",
                SubjectId = "123",
                Scopes = new string[] { "foo3" }
            });
            await _store.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client1",
                SubjectId = "456",
                Scopes = new string[] { "foo3" }
            });

            await _store.StoreReferenceTokenAsync("key1", new Token()
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
            await _store.StoreReferenceTokenAsync("key2", new Token()
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
            await _store.StoreReferenceTokenAsync("key3", new Token()
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

            await _store.StoreRefreshTokenAsync("key4", new RefreshToken()
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
            await _store.StoreRefreshTokenAsync("key5", new RefreshToken()
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
            await _store.StoreRefreshTokenAsync("key6", new RefreshToken()
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

            await _store.StoreAuthorizationCodeAsync("key7", new AuthorizationCode()
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
            await _store.StoreAuthorizationCodeAsync("key8", new AuthorizationCode()
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
            
            await _store.StoreAuthorizationCodeAsync("key9", new AuthorizationCode()
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
            await _store.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client1",
                SubjectId = "123",
                Scopes = new string[] { "foo1", "foo2" }
            });
            await _store.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client2",
                SubjectId = "123",
                Scopes = new string[] { "foo3" }
            });
            await _store.StoreUserConsentAsync(new Consent()
            {
                ClientId = "client1",
                SubjectId = "456",
                Scopes = new string[] { "foo3" }
            });

            await _store.StoreReferenceTokenAsync("key1", new Token()
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
            await _store.StoreReferenceTokenAsync("key2", new Token()
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
            await _store.StoreReferenceTokenAsync("key3", new Token()
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

            await _store.StoreRefreshTokenAsync("key4", new RefreshToken()
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
            await _store.StoreRefreshTokenAsync("key5", new RefreshToken()
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
            await _store.StoreRefreshTokenAsync("key6", new RefreshToken()
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

            await _store.StoreAuthorizationCodeAsync("key7", new AuthorizationCode()
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
            await _store.StoreAuthorizationCodeAsync("key8", new AuthorizationCode()
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

            await _store.StoreAuthorizationCodeAsync("key9", new AuthorizationCode()
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

            (await _store.GetReferenceTokenAsync("key1")).Should().BeNull();
            (await _store.GetReferenceTokenAsync("key2")).Should().NotBeNull();
            (await _store.GetReferenceTokenAsync("key3")).Should().NotBeNull();
            (await _store.GetRefreshTokenAsync("key4")).Should().BeNull();
            (await _store.GetRefreshTokenAsync("key5")).Should().NotBeNull();
            (await _store.GetRefreshTokenAsync("key6")).Should().NotBeNull();
            (await _store.GetAuthorizationCodeAsync("key7")).Should().BeNull();
            (await _store.GetAuthorizationCodeAsync("key8")).Should().NotBeNull();
            (await _store.GetAuthorizationCodeAsync("key9")).Should().NotBeNull();
        }
    }
}
