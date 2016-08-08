﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Services.InMemory
{
    /// <summary>
    /// In-memory consent store
    /// </summary>
    public class InMemoryConsentStore : IConsentStore
    {
        private readonly List<Consent> _consents = new List<Consent>();

        /// <summary>
        /// Loads all permissions the subject has granted to all clients.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>The permissions.</returns>
        public Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            var query =
                from c in _consents
                where c.Subject == subject
                select c;
            return Task.FromResult<IEnumerable<Consent>>(query.ToArray());
        }

        /// <summary>
        /// Loads the subject's prior consent for the client.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns>The persisted consent.</returns>
        public Task<Consent> LoadAsync(string subject, string client)
        {
            var query =
                from c in _consents
                where c.Subject == subject && c.ClientId == client
                select c;
            return Task.FromResult(query.SingleOrDefault());
        }


        /// <summary>
        /// Persists the subject's consent.
        /// </summary>
        /// <param name="consent">The consent.</param>
        /// <returns></returns>
        public Task UpdateAsync(Consent consent)
        {
            // makes a snapshot as a DB would
            consent.Scopes = consent.Scopes.ToArray();

            var query =
                from c in _consents
                where c.Subject == consent.Subject && c.ClientId == consent.ClientId
                select c;
            var item = query.SingleOrDefault();
            if (item != null)
            {
                item.Scopes = consent.Scopes;
            }
            else
            {
                _consents.Add(consent);
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Revokes all permissions the subject has given to a client.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public Task RevokeAsync(string subject, string client)
        {
            var query =
                from c in _consents
                where c.Subject == subject && c.ClientId == client
                select c;
            var item = query.SingleOrDefault();
            if (item != null)
            {
                _consents.Remove(item);
            }
            return Task.FromResult(0);
        }
    }
}