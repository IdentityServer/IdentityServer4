﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Xunit;

namespace IdentityServer4.EntityFramework.IntegrationTests.Stores
{
    public class PersistedGrantStoreTests : IntegrationTest<PersistedGrantStoreTests, PersistedGrantDbContext, OperationalStoreOptions>
    {
        public PersistedGrantStoreTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
        {
            foreach (var options in TestDatabaseProviders.SelectMany(x => x.Select(y => (DbContextOptions<PersistedGrantDbContext>)y)).ToList())
            {
                using (var context = new PersistedGrantDbContext(options, StoreOptions))
                    context.Database.EnsureCreated();
            }
        }

        private static PersistedGrant CreateTestObject(string sub = null, string clientId = null, string sid = null, string type = null)
        {
            return new PersistedGrant
            {
                Key = Guid.NewGuid().ToString(),
                Type = type ?? "authorization_code",
                ClientId = clientId ?? Guid.NewGuid().ToString(),
                SubjectId = sub ?? Guid.NewGuid().ToString(),
                SessionId = sid ?? Guid.NewGuid().ToString(),
                CreationTime = new DateTime(2016, 08, 01),
                Expiration = new DateTime(2016, 08, 31),
                Data = Guid.NewGuid().ToString()
            };
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task StoreAsync_WhenPersistedGrantStored_ExpectSuccess(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.StoreAsync(persistedGrant);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task GetAsync_WithKeyAndPersistedGrantExists_ExpectPersistedGrantReturned(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                context.SaveChanges();
            }

            PersistedGrant foundPersistedGrant;
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                foundPersistedGrant = await store.GetAsync(persistedGrant.Key);
            }

            Assert.NotNull(foundPersistedGrant);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task GetAllAsync_WithSubAndTypeAndPersistedGrantExists_ExpectPersistedGrantReturned(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                context.SaveChanges();
            }

            IList<PersistedGrant> foundPersistedGrants;
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                foundPersistedGrants = (await store.GetAllAsync(new PersistedGrantFilter { SubjectId = persistedGrant.SubjectId })).ToList();
            }

            Assert.NotNull(foundPersistedGrants);
            Assert.NotEmpty(foundPersistedGrants);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task GetAllAsync_Should_Filter(DbContextOptions<PersistedGrantDbContext> options)
        {
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t1").ToEntity());
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t2").ToEntity());
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t1").ToEntity());
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t2").ToEntity());
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t1").ToEntity());
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t2").ToEntity());
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t1").ToEntity());
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t2").ToEntity());
                context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c3", sid: "s3", type: "t3").ToEntity());
                context.PersistedGrants.Add(CreateTestObject().ToEntity());
                context.SaveChanges();
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1"
                })).ToList().Count.Should().Be(9);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub2"
                })).ToList().Count.Should().Be(0);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c1"
                })).ToList().Count.Should().Be(4);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c2"
                })).ToList().Count.Should().Be(4);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c3"
                })).ToList().Count.Should().Be(1);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c4"
                })).ToList().Count.Should().Be(0);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c1",
                    SessionId = "s1"
                })).ToList().Count.Should().Be(2);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c3",
                    SessionId = "s1"
                })).ToList().Count.Should().Be(0);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c1",
                    SessionId = "s1",
                    Type = "t1"
                })).ToList().Count.Should().Be(1);
                (await store.GetAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c1",
                    SessionId = "s1",
                    Type = "t3"
                })).ToList().Count.Should().Be(0);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveAsync_WhenKeyOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                context.SaveChanges();
            }
            
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.RemoveAsync(persistedGrant.Key);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveAllAsync_WhenSubIdAndClientIdOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                context.SaveChanges();
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.RemoveAllAsync(new PersistedGrantFilter { 
                    SubjectId = persistedGrant.SubjectId, 
                    ClientId = persistedGrant.ClientId 
                });
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveAllAsync_WhenSubIdClientIdAndTypeOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                context.SaveChanges();
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.RemoveAllAsync(new PersistedGrantFilter { 
                    SubjectId = persistedGrant.SubjectId, 
                    ClientId = persistedGrant.ClientId, 
                    Type = persistedGrant.Type });
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }


        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveAllAsync_Should_Filter(DbContextOptions<PersistedGrantDbContext> options)
        {
            void PopulateDb()
            {
                using (var context = new PersistedGrantDbContext(options, StoreOptions))
                {
                    context.PersistedGrants.RemoveRange(context.PersistedGrants.ToArray());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t1").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t2").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t1").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t2").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t1").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t2").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t1").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t2").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c3", sid: "s3", type: "t3").ToEntity());
                    context.PersistedGrants.Add(CreateTestObject().ToEntity());
                    context.SaveChanges();
                }
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1"
                });
                context.PersistedGrants.Count().Should().Be(1);
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub2"
                });
                context.PersistedGrants.Count().Should().Be(10);
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1", ClientId = "c1"
                });
                context.PersistedGrants.Count().Should().Be(6);
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c2"
                });
                context.PersistedGrants.Count().Should().Be(6);
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c3"
                });
                context.PersistedGrants.Count().Should().Be(9);
            }


            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c4"
                });
                context.PersistedGrants.Count().Should().Be(10);
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c1", 
                    SessionId = "s1"
                });
                context.PersistedGrants.Count().Should().Be(8);
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c3",
                    SessionId = "s1"
                });
                context.PersistedGrants.Count().Should().Be(10);
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c1",
                    SessionId = "s1", 
                    Type = "t1"
                });
                context.PersistedGrants.Count().Should().Be(9);
            }

            PopulateDb();
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

                await store.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = "sub1",
                    ClientId = "c1",
                    SessionId = "s1",
                    Type = "t3"
                });
                context.PersistedGrants.Count().Should().Be(10);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task Store_should_create_new_record_if_key_does_not_exist(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.StoreAsync(persistedGrant);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task Store_should_update_record_if_key_already_exists(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                context.SaveChanges();
            }

            var newDate = persistedGrant.Expiration.Value.AddHours(1);
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                persistedGrant.Expiration = newDate;
                await store.StoreAsync(persistedGrant);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
                Assert.Equal(newDate, persistedGrant.Expiration);
            }
        }
    }
}