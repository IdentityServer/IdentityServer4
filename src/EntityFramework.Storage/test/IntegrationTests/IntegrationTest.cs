﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IdentityServer4.EntityFramework.IntegrationTests
{
    /// <summary>
    /// Base class for integration tests, responsible for initializing test database providers & an xUnit class fixture
    /// </summary>
    /// <typeparam name="TClass">The type of the class.</typeparam>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TStoreOption">The type of the store option.</typeparam>
    /// <seealso cref="DatabaseProviderFixture{T}" />
    public class IntegrationTest<TClass, TDbContext, TStoreOption> : IClassFixture<DatabaseProviderFixture<TDbContext>>
        where TDbContext : DbContext
    {
        public static readonly TheoryData<DbContextOptions<TDbContext>> TestDatabaseProviders;
        protected readonly TStoreOption StoreOptions = Activator.CreateInstance<TStoreOption>();

        static IntegrationTest()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            if (config.GetValue("APPVEYOR", false))
            {
                Console.WriteLine($"Running AppVeyor Tests for {typeof(TClass).Name}");

                TestDatabaseProviders = new TheoryData<DbContextOptions<TDbContext>>
                {
                    DatabaseProviderBuilder.BuildInMemory<TDbContext>(typeof(TClass).Name),
                    DatabaseProviderBuilder.BuildSqlite<TDbContext>(typeof(TClass).Name),

                    //DatabaseProviderBuilder.BuildAppVeyorMySql<TDbContext>(typeof(TClass).Name),
                    //DatabaseProviderBuilder.BuildAppVeyorPostgreSql<TDbContext>(typeof(TClass).Name)
                };

                if (Environment.OSVersion.Platform != PlatformID.Unix)
                {
                    TestDatabaseProviders.Add(DatabaseProviderBuilder.BuildAppVeyorSqlServer2016<TDbContext>(typeof(TClass).Name));
                }
            }
            else
            {
                if (Environment.OSVersion.Platform != PlatformID.Unix)
                {
                    Console.WriteLine($"Running Local Tests for {typeof(TClass).Name}");

                    TestDatabaseProviders = new TheoryData<DbContextOptions<TDbContext>>
                    {
                        DatabaseProviderBuilder.BuildInMemory<TDbContext>(typeof(TClass).Name),
                        DatabaseProviderBuilder.BuildSqlite<TDbContext>(typeof(TClass).Name),
                        DatabaseProviderBuilder.BuildLocalDb<TDbContext>(typeof(TClass).Name)
                    };
                }
            }
        }

        protected IntegrationTest(DatabaseProviderFixture<TDbContext> fixture)
        {
            fixture.Options = TestDatabaseProviders.SelectMany(x => x.Select(y => (DbContextOptions<TDbContext>)y)).ToList();
            fixture.StoreOptions = StoreOptions;
        }
    }
}