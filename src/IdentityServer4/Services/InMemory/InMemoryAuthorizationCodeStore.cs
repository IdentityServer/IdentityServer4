// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services.InMemory
{
    /// <summary>
    /// In-memory authorization code store
    /// </summary>
    public class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly ConcurrentDictionary<string, AuthorizationCode> _repository = new ConcurrentDictionary<string, AuthorizationCode>();

        /// <summary>
        /// Stores the data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Task StoreAsync(string key, AuthorizationCode value)
        {
            _repository[key] = value;

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Retrieves the data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Task<AuthorizationCode> GetAsync(string key)
        {
            AuthorizationCode code;
            if (_repository.TryGetValue(key, out code))
            {
                return Task.FromResult(code);
            }

            return Task.FromResult<AuthorizationCode>(null);
        }

        /// <summary>
        /// Removes the data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Task RemoveAsync(string key)
        {
            AuthorizationCode val;
            _repository.TryRemove(key, out val);

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Retrieves all data for a subject identifier.
        /// </summary>
        /// <param name="subject">The subject identifier.</param>
        /// <returns>
        /// A list of token metadata
        /// </returns>
        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var query =
                from item in _repository
                where item.Value.SubjectId == subject
                select item.Value;
            var list = query.ToArray();
            return Task.FromResult(list.Cast<ITokenMetadata>());
        }

        /// <summary>
        /// Revokes all data for a client and subject id combination.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public Task RevokeAsync(string subject, string client)
        {
            var query =
                from item in _repository
                where item.Value.Subject.GetSubjectId() == subject && item.Value.ClientId == client
                select item.Key;

            foreach (var key in query)
            {
                RemoveAsync(key);
            }

            return Task.FromResult(0);
        }
    }
}