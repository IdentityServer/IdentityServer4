// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace IdentityServer4.Stores.InMemory
{
    public class InMemoryValidationKeysStore : IValidationKeysStore
    {
        private readonly IEnumerable<SecurityKey> _keys;

        public InMemoryValidationKeysStore(IEnumerable<SecurityKey> keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            _keys = keys;
        }

        public Task<IEnumerable<SecurityKey>> GetValidationKeysAsync()
        {
            return Task.FromResult(_keys);
        }
    }
}