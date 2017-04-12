﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default persisted grant service
    /// </summary>
    public class DefaultPersistedGrantService : IPersistedGrantService
    {
        private readonly ILogger<DefaultPersistedGrantService> _logger;
        private readonly IPersistedGrantStore _store;
        private readonly IPersistentGrantSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPersistedGrantService"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="logger">The logger.</param>
        public DefaultPersistedGrantService(IPersistedGrantStore store, 
            IPersistentGrantSerializer serializer,
            ILogger<DefaultPersistedGrantService> logger)
        {
            _store = store;
            _serializer = serializer;
            _logger = logger;
        }

        /// <summary>
        /// Gets all grants for a given subject ID.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Consent>> GetAllGrantsAsync(string subjectId)
        {
            var grants = (await _store.GetAllAsync(subjectId)).ToArray();

            try
            {
                var consents = grants.Where(x => x.Type == Constants.PersistedGrantTypes.UserConsent)
                    .Select(x => _serializer.Deserialize<Consent>(x.Data));

                var codes = grants.Where(x => x.Type == Constants.PersistedGrantTypes.AuthorizationCode)
                    .Select(x => _serializer.Deserialize<AuthorizationCode>(x.Data))
                    .Select(x => new Consent
                    {
                        ClientId = x.ClientId,
                        SubjectId = subjectId,
                        Scopes = x.RequestedScopes,
                        CreationTime = x.CreationTime,
                        Expiration = x.CreationTime.AddSeconds(x.Lifetime)
                    });

                var refresh = grants.Where(x => x.Type == Constants.PersistedGrantTypes.RefreshToken)
                    .Select(x => _serializer.Deserialize<RefreshToken>(x.Data))
                    .Select(x => new Consent
                    {
                        ClientId = x.ClientId,
                        SubjectId = subjectId,
                        Scopes = x.Scopes,
                        CreationTime = x.CreationTime,
                        Expiration = x.CreationTime.AddSeconds(x.Lifetime)
                    });

                var access = grants.Where(x => x.Type == Constants.PersistedGrantTypes.ReferenceToken)
                    .Select(x => _serializer.Deserialize<Token>(x.Data))
                    .Select(x => new Consent
                    {
                        ClientId = x.ClientId,
                        SubjectId = subjectId,
                        Scopes = x.Scopes,
                        CreationTime = x.CreationTime,
                        Expiration = x.CreationTime.AddSeconds(x.Lifetime)
                    });

                consents = Join(consents, codes);
                consents = Join(consents, refresh);
                consents = Join(consents, access);

                return consents.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed processing results from grant store. Exception: {0}", ex.Message);
            }

            return Enumerable.Empty<Consent>();
        }

        IEnumerable<Consent> Join(IEnumerable<Consent> first, IEnumerable<Consent> second)
        {
            var query =
                from f in first
                let matches = (from s in second where f.ClientId == s.ClientId from scope in s.Scopes select scope)
                let scopes = f.Scopes.Union(matches).Distinct()
                select new Consent
                {
                    ClientId = f.ClientId,
                    SubjectId = f.SubjectId,
                    Scopes = scopes,
                    CreationTime = f.CreationTime,
                    Expiration = f.Expiration
                };
            return query;
        }

        /// <summary>
        /// Removes all grants for a given subject id and client id combination.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public Task RemoveAllGrantsAsync(string subjectId, string clientId)
        {
            return _store.RemoveAllAsync(subjectId, clientId);
        }
    }
}