// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public class DefaultHashingService : IHashingService
    {
        public Task<string> ComputeHashAsync(string value)
        {
            return Task.FromResult(value.Sha256());
        }
    }
}
