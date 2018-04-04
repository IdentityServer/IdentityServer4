// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    public class ValidatingClientStore<T> : IClientStore
        where T : IClientStore
    {
        private readonly IClientStore _inner;
        private readonly IEventService _events;

        public ValidatingClientStore(T inner, IClientConfigurationValidator validator, IEventService events)
        {
            _inner = inner;
            _events = events;
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return _inner.FindClientByIdAsync(clientId);
        }
    }
}