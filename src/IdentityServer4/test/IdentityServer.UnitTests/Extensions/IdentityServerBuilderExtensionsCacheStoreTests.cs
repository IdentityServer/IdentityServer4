﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityServer.UnitTests.Extensions
{
    public class IdentityServerBuilderExtensionsCacheStoreTests
    {
        private class CustomClientStore: IClientStore
        {
            public Task<Client> FindClientByIdAsync(string clientId)
            {
                throw new System.NotImplementedException();
            }
        }

        private class CustomResourceStore : IResourceStore
        {
            public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
            {
                throw new System.NotImplementedException();
            }

            public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
            {
                throw new System.NotImplementedException();
            }

            public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> names)
            {
                throw new System.NotImplementedException();
            }

            public Task<Resources> GetAllResourcesAsync()
            {
                throw new System.NotImplementedException();
            }

            public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public void AddClientStoreCache_should_add_concrete_iclientstore_implementation()
        {
            var services = new ServiceCollection();
            var identityServerBuilder = new IdentityServerBuilder(services);

            identityServerBuilder.AddClientStoreCache<CustomClientStore>();

            services.Any(x => x.ImplementationType == typeof(CustomClientStore)).Should().BeTrue();
        }

        [Fact]
        public void AddResourceStoreCache_should_attempt_to_register_iresourcestore_implementation()
        {
            var services = new ServiceCollection();
            var identityServerBuilder = new IdentityServerBuilder(services);

            identityServerBuilder.AddResourceStoreCache<CustomResourceStore>();

            services.Any(x => x.ImplementationType == typeof(CustomResourceStore)).Should().BeTrue();
        }
    }
}