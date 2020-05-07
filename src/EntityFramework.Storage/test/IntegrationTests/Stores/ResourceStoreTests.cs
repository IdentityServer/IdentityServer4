﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityServer4.EntityFramework.IntegrationTests.Stores
{
    public class ScopeStoreTests : IntegrationTest<ScopeStoreTests, ConfigurationDbContext, ConfigurationStoreOptions>
    {
        public ScopeStoreTests(DatabaseProviderFixture<ConfigurationDbContext> fixture) : base(fixture)
        {
            foreach (var options in TestDatabaseProviders.SelectMany(x => x.Select(y => (DbContextOptions<ConfigurationDbContext>)y)).ToList())
            {
                using (var context = new ConfigurationDbContext(options, StoreOptions))
                    context.Database.EnsureCreated();
            }
        }

        private static IdentityResource CreateIdentityTestResource()
        {
            return new IdentityResource()
            {
                Name = Guid.NewGuid().ToString(),
                DisplayName = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ShowInDiscoveryDocument = true,
                UserClaims = 
                {
                    JwtClaimTypes.Subject,
                    JwtClaimTypes.Name,
                }
            };
        }

        private static ApiResource CreateApiResourceTestResource()
        {
            return new ApiResource()
            {
                Name = Guid.NewGuid().ToString(),
                ApiSecrets = new List<Secret> { new Secret("secret".ToSha256()) },
                Scopes = { Guid.NewGuid().ToString() },
                UserClaims =
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }
            };
        }
        
        private static ApiScope CreateApiScopeTestResource()
        {
            return new ApiScope()
            {
                Name = Guid.NewGuid().ToString(),
                UserClaims =
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }
            };
        }


        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task FindApiResourcesByNameAsync_WhenResourceExists_ExpectResourceAndCollectionsReturned(DbContextOptions<ConfigurationDbContext> options)
        {
            var resource = CreateApiResourceTestResource();

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.ApiResources.Add(resource.ToEntity());
                context.SaveChanges();
            }

            ApiResource foundResource;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                foundResource = (await store.FindApiResourcesByNameAsync(new[] { resource.Name })).SingleOrDefault();
            }

            Assert.NotNull(foundResource);
            Assert.True(foundResource.Name == resource.Name);

            Assert.NotNull(foundResource.UserClaims);
            Assert.NotEmpty(foundResource.UserClaims);
            Assert.NotNull(foundResource.ApiSecrets);
            Assert.NotEmpty(foundResource.ApiSecrets);
            Assert.NotNull(foundResource.Scopes);
            Assert.NotEmpty(foundResource.Scopes);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task FindApiResourcesByNameAsync_WhenResourcesExist_ExpectOnlyResourcesRequestedReturned(DbContextOptions<ConfigurationDbContext> options)
        {
            var resource = CreateApiResourceTestResource();

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.ApiResources.Add(resource.ToEntity());
                context.ApiResources.Add(CreateApiResourceTestResource().ToEntity());
                context.SaveChanges();
            }

            ApiResource foundResource;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                foundResource = (await store.FindApiResourcesByNameAsync(new[] { resource.Name })).SingleOrDefault();
            }

            Assert.NotNull(foundResource);
            Assert.True(foundResource.Name == resource.Name);

            Assert.NotNull(foundResource.UserClaims);
            Assert.NotEmpty(foundResource.UserClaims);
            Assert.NotNull(foundResource.ApiSecrets);
            Assert.NotEmpty(foundResource.ApiSecrets);
            Assert.NotNull(foundResource.Scopes);
            Assert.NotEmpty(foundResource.Scopes);
        }




        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task FindApiResourcesByScopeNameAsync_WhenResourcesExist_ExpectResourcesReturned(DbContextOptions<ConfigurationDbContext> options)
        {
            var testApiResource = CreateApiResourceTestResource();
            var testApiScope = CreateApiScopeTestResource();
            testApiResource.Scopes.Add(testApiScope.Name);

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.ApiResources.Add(testApiResource.ToEntity());
                context.ApiScopes.Add(testApiScope.ToEntity());
                context.SaveChanges();
            }

            IEnumerable<ApiResource> resources;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                resources = await store.FindApiResourcesByScopeNameAsync(new List<string>
                {
                    testApiScope.Name
                });
            }

            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == testApiResource.Name));
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task FindApiResourcesByScopeNameAsync_WhenResourcesExist_ExpectOnlyResourcesRequestedReturned(DbContextOptions<ConfigurationDbContext> options)
        {
            var testIdentityResource = CreateIdentityTestResource();
            var testApiResource = CreateApiResourceTestResource();
            var testApiScope = CreateApiScopeTestResource();
            testApiResource.Scopes.Add(testApiScope.Name);

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.IdentityResources.Add(testIdentityResource.ToEntity());
                context.ApiResources.Add(testApiResource.ToEntity());
                context.ApiScopes.Add(testApiScope.ToEntity());
                context.IdentityResources.Add(CreateIdentityTestResource().ToEntity());
                context.ApiResources.Add(CreateApiResourceTestResource().ToEntity());
                context.ApiScopes.Add(CreateApiScopeTestResource().ToEntity());
                context.SaveChanges();
            }

            IEnumerable<ApiResource> resources;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                resources = await store.FindApiResourcesByScopeNameAsync(new[] { testApiScope.Name });
            }

            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == testApiResource.Name));
        }




        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task FindIdentityResourcesByScopeNameAsync_WhenResourceExists_ExpectResourceAndCollectionsReturned(DbContextOptions<ConfigurationDbContext> options)
        {
            var resource = CreateIdentityTestResource();

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.IdentityResources.Add(resource.ToEntity());
                context.SaveChanges();
            }

            IList<IdentityResource> resources;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                resources = (await store.FindIdentityResourcesByScopeNameAsync(new List<string>
                {
                    resource.Name
                })).ToList();
            }

            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            var foundScope = resources.Single();

            Assert.Equal(resource.Name, foundScope.Name);
            Assert.NotNull(foundScope.UserClaims);
            Assert.NotEmpty(foundScope.UserClaims);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task FindIdentityResourcesByScopeNameAsync_WhenResourcesExist_ExpectOnlyRequestedReturned(DbContextOptions<ConfigurationDbContext> options)
        {
            var resource = CreateIdentityTestResource();

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.IdentityResources.Add(resource.ToEntity());
                context.IdentityResources.Add(CreateIdentityTestResource().ToEntity());
                context.SaveChanges();
            }

            IList<IdentityResource> resources;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                resources = (await store.FindIdentityResourcesByScopeNameAsync(new List<string>
                {
                    resource.Name
                })).ToList();
            }

            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == resource.Name));
        }



        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task FindApiScopesByNameAsync_WhenResourceExists_ExpectResourceAndCollectionsReturned(DbContextOptions<ConfigurationDbContext> options)
        {
            var resource = CreateApiScopeTestResource();

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.ApiScopes.Add(resource.ToEntity());
                context.SaveChanges();
            }

            IList<ApiScope> resources;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                resources = (await store.FindApiScopesByNameAsync(new List<string>
                {
                    resource.Name
                })).ToList();
            }

            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            var foundScope = resources.Single();

            Assert.Equal(resource.Name, foundScope.Name);
            Assert.NotNull(foundScope.UserClaims);
            Assert.NotEmpty(foundScope.UserClaims);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task FindApiScopesByNameAsync_WhenResourcesExist_ExpectOnlyRequestedReturned(DbContextOptions<ConfigurationDbContext> options)
        {
            var resource = CreateApiScopeTestResource();

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.ApiScopes.Add(resource.ToEntity());
                context.ApiScopes.Add(CreateApiScopeTestResource().ToEntity());
                context.SaveChanges();
            }

            IList<ApiScope> resources;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                resources = (await store.FindApiScopesByNameAsync(new List<string>
                {
                    resource.Name
                })).ToList();
            }

            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == resource.Name));
        }




        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task GetAllResources_WhenAllResourcesRequested_ExpectAllResourcesIncludingHidden(DbContextOptions<ConfigurationDbContext> options)
        {
            var visibleIdentityResource = CreateIdentityTestResource();
            var visibleApiResource = CreateApiResourceTestResource();
            var visibleApiScope = CreateApiScopeTestResource();
            var hiddenIdentityResource = new IdentityResource { Name = Guid.NewGuid().ToString(), ShowInDiscoveryDocument = false };
            var hiddenApiResource = new ApiResource
            {
                Name = Guid.NewGuid().ToString(),
                Scopes = { Guid.NewGuid().ToString() },
                ShowInDiscoveryDocument = false
            };
            var hiddenApiScope = new ApiScope
            {
                Name = Guid.NewGuid().ToString(),
                ShowInDiscoveryDocument = false
            };

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.IdentityResources.Add(visibleIdentityResource.ToEntity());
                context.ApiResources.Add(visibleApiResource.ToEntity());
                context.ApiScopes.Add(visibleApiScope.ToEntity());

                context.IdentityResources.Add(hiddenIdentityResource.ToEntity());
                context.ApiResources.Add(hiddenApiResource.ToEntity());
                context.ApiScopes.Add(hiddenApiScope.ToEntity());
                
                context.SaveChanges();
            }

            Resources resources;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ResourceStore(context, FakeLogger<ResourceStore>.Create());
                resources = await store.GetAllResourcesAsync();
            }

            Assert.NotNull(resources);
            Assert.NotEmpty(resources.IdentityResources);
            Assert.NotEmpty(resources.ApiResources);
            Assert.NotEmpty(resources.ApiScopes);

            Assert.Contains(resources.IdentityResources, x => x.Name == visibleIdentityResource.Name);
            Assert.Contains(resources.IdentityResources, x => x.Name == hiddenIdentityResource.Name);

            Assert.Contains(resources.ApiResources, x => x.Name == visibleApiResource.Name);
            Assert.Contains(resources.ApiResources, x => x.Name == hiddenApiResource.Name);

            Assert.Contains(resources.ApiScopes, x => x.Name == visibleApiScope.Name);
            Assert.Contains(resources.ApiScopes, x => x.Name == hiddenApiScope.Name);
        }
    }
}