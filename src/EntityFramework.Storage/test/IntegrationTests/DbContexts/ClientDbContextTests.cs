﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Options;
using Xunit;

namespace IdentityServer4.EntityFramework.IntegrationTests.DbContexts
{
    public class ClientDbContextTests : IntegrationTest<ClientDbContextTests, ConfigurationDbContext, ConfigurationStoreOptions>
    {
        public ClientDbContextTests(DatabaseProviderFixture<ConfigurationDbContext> fixture) : base(fixture)
        {
            foreach (var options in TestDatabaseProviders.SelectMany(x => x.Select(y => (DbContextOptions<ConfigurationDbContext>)y)).ToList())
            {
                using (var context = new ConfigurationDbContext(options, StoreOptions))
                    context.Database.EnsureCreated();
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void CanAddAndDeleteClientScopes(DbContextOptions<ConfigurationDbContext> options)
        {
            using (var db = new ConfigurationDbContext(options, StoreOptions))
            {
                db.Clients.Add(new Client
                {
                    ClientId = "test-client-scopes",
                    ClientName = "Test Client"
                });

                db.SaveChanges();
            }

            using (var db = new ConfigurationDbContext(options, StoreOptions))
            {
                // explicit include due to lack of EF Core lazy loading
                var client = db.Clients.Include(x => x.AllowedScopes).First();

                client.AllowedScopes.Add(new ClientScope
                {
                    Scope = "test"
                });

                db.SaveChanges();
            }

            using (var db = new ConfigurationDbContext(options, StoreOptions))
            {
                var client = db.Clients.Include(x => x.AllowedScopes).First();
                var scope = client.AllowedScopes.First();

                client.AllowedScopes.Remove(scope);

                db.SaveChanges();
            }

            using (var db = new ConfigurationDbContext(options, StoreOptions))
            {
                var client = db.Clients.Include(x => x.AllowedScopes).First();

                Assert.Empty(client.AllowedScopes);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void CanAddAndDeleteClientRedirectUri(DbContextOptions<ConfigurationDbContext> options)
        {
            using (var db = new ConfigurationDbContext(options, StoreOptions))
            {
                db.Clients.Add(new Client
                {
                    ClientId = "test-client",
                    ClientName = "Test Client"
                });

                db.SaveChanges();
            }

            using (var db = new ConfigurationDbContext(options, StoreOptions))
            {
                var client = db.Clients.Include(x => x.RedirectUris).First();

                client.RedirectUris.Add(new ClientRedirectUri
                {
                    RedirectUri = "https://redirect-uri-1"
                });

                db.SaveChanges();
            }

            using (var db = new ConfigurationDbContext(options, StoreOptions))
            {
                var client = db.Clients.Include(x => x.RedirectUris).First();
                var redirectUri = client.RedirectUris.First();

                client.RedirectUris.Remove(redirectUri);

                db.SaveChanges();
            }

            using (var db = new ConfigurationDbContext(options, StoreOptions))
            {
                var client = db.Clients.Include(x => x.RedirectUris).First();

                Assert.Empty(client.RedirectUris);
            }
        }
    }
}