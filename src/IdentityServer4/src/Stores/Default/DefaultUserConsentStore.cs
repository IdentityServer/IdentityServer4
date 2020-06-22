// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using System.Threading;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Default user consent store.
    /// </summary>
    public class DefaultUserConsentStore : DefaultGrantStore<Consent>, IUserConsentStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUserConsentStore"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        public DefaultUserConsentStore(
            IPersistedGrantStore store, 
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            ILogger<DefaultUserConsentStore> logger) 
            : base(IdentityServerConstants.PersistedGrantTypes.UserConsent, store, serializer, handleGenerationService, logger)
        {
        }

        private string GetConsentKey(string subjectId, string clientId)
        {
            return clientId + "|" + subjectId;
        }

        /// <inheritdoc/>
        public Task StoreUserConsentAsync(Consent consent, CancellationToken cancellationToken = default)
        {
            var key = GetConsentKey(consent.SubjectId, consent.ClientId);
            return StoreItemAsync(key, consent, consent.ClientId, consent.SubjectId, null, null, consent.CreationTime, consent.Expiration, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public Task<Consent> GetUserConsentAsync(string subjectId, string clientId, CancellationToken cancellationToken = default)
        {
            var key = GetConsentKey(subjectId, clientId);
            return GetItemAsync(key, cancellationToken);
        }

        /// <inheritdoc/>
        public Task RemoveUserConsentAsync(string subjectId, string clientId, CancellationToken cancellationToken = default)
        {
            var key = GetConsentKey(subjectId, clientId);
            return RemoveItemAsync(key, cancellationToken);
        }
    }
}
