// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
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

        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client
        /// </returns>
        public virtual async Task<Client> FindClientByIdAsync(string clientId)
        {
            var exists = await _context.Clients
                .CountAsync(x => x.ClientId == clientId);

            if (exists != 1) return null;

            var baseQuery = _context.Clients.AsTracking()
                .Where(x => x.ClientId == clientId);

            var client = await baseQuery.FirstOrDefaultAsync();

            if (client == null) return null;

            await _context.Entry(client).Collection(x => x.AllowedCorsOrigins).LoadAsync();
            await _context.Entry(client).Collection(x => x.AllowedGrantTypes).LoadAsync();
            await _context.Entry(client).Collection(x => x.AllowedScopes).LoadAsync();
            await _context.Entry(client).Collection(x => x.Claims).LoadAsync();
            await _context.Entry(client).Collection(x => x.ClientSecrets).LoadAsync();
            await _context.Entry(client).Collection(x => x.IdentityProviderRestrictions).LoadAsync();
            await _context.Entry(client).Collection(x => x.PostLogoutRedirectUris).LoadAsync();
            await _context.Entry(client).Collection(x => x.Properties).LoadAsync();
            await _context.Entry(client).Collection(x => x.RedirectUris).LoadAsync();

            var model = client.ToModel();

            return model;
        }
    }
}
