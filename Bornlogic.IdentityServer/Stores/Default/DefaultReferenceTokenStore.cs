﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Bornlogic.IdentityServer.Services;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;
using Bornlogic.IdentityServer.Storage.Stores.Serialization;
using Microsoft.Extensions.Logging;

namespace Bornlogic.IdentityServer.Stores.Default
{
    /// <summary>
    /// Default reference token store.
    /// </summary>
    public class DefaultReferenceTokenStore : DefaultGrantStore<Token>, IReferenceTokenStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultReferenceTokenStore"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        public DefaultReferenceTokenStore(
            IPersistedGrantStore store, 
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            ILogger<DefaultReferenceTokenStore> logger) 
            : base(IdentityServerConstants.PersistedGrantTypes.ReferenceToken, store, serializer, handleGenerationService, logger)
        {
        }

        /// <summary>
        /// Stores the reference token asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Task<string> StoreReferenceTokenAsync(Token token)
        {
            return CreateItemAsync(token, token.ClientId, token.SubjectId, token.SessionId, token.Description, token.CreationTime, token.Lifetime);
        }

        /// <summary>
        /// Gets the reference token asynchronous.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        public Task<Token> GetReferenceTokenAsync(string handle)
        {
            return GetItemAsync(handle);
        }

        /// <summary>
        /// Removes the reference token asynchronous.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        public Task RemoveReferenceTokenAsync(string handle)
        {
            return RemoveItemAsync(handle);
        }

        /// <summary>
        /// Removes the reference tokens asynchronous.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public Task RemoveReferenceTokensAsync(string subjectId, string clientId)
        {
            return RemoveAllAsync(subjectId, clientId);
        }
    }
}