// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.EntityFramework.Stores
{
    /// <summary>
    /// Implementation of IClientStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IClientStore" />
    public class ClientStore : IClientStore
    {
        /// <summary>
        /// The DbContext.
        /// </summary>
        protected readonly IConfigurationDbContext Context;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger<ClientStore> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">context</exception>
        public ClientStore(IConfigurationDbContext context, ILogger<ClientStore> logger)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task<Client> FindClientByIdAsync(string clientId, CancellationToken cancellationToken = default)
        {
            IQueryable<Entities.Client> baseQuery = Context.Clients
                .Where(x => x.ClientId == clientId);

            var client = (await baseQuery.ToArrayAsync(cancellationToken))
                .SingleOrDefault(x => x.ClientId == clientId);

            if (client == null) return null;

            await baseQuery.Include(x => x.AllowedCorsOrigins).SelectMany(c => c.AllowedCorsOrigins).LoadAsync(cancellationToken);
            await baseQuery.Include(x => x.AllowedGrantTypes).SelectMany(c => c.AllowedGrantTypes).LoadAsync(cancellationToken);
            await baseQuery.Include(x => x.AllowedScopes).SelectMany(c => c.AllowedScopes).LoadAsync(cancellationToken);
            await baseQuery.Include(x => x.Claims).SelectMany(c => c.Claims).LoadAsync(cancellationToken);
            await baseQuery.Include(x => x.ClientSecrets).SelectMany(c => c.ClientSecrets).LoadAsync(cancellationToken);
            await baseQuery.Include(x => x.IdentityProviderRestrictions).SelectMany(c => c.IdentityProviderRestrictions).LoadAsync(cancellationToken);
            await baseQuery.Include(x => x.PostLogoutRedirectUris).SelectMany(c => c.PostLogoutRedirectUris).LoadAsync(cancellationToken);
            await baseQuery.Include(x => x.Properties).SelectMany(c => c.Properties).LoadAsync(cancellationToken);
            await baseQuery.Include(x => x.RedirectUris).SelectMany(c => c.RedirectUris).LoadAsync(cancellationToken);

            var model = client.ToModel();

            Logger.LogDebug("{clientId} found in database: {clientIdFound}", clientId, model != null);

            return model;
        }
    }
}