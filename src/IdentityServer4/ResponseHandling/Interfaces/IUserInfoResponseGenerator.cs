// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.ResponseHandling
{
    public interface IUserInfoResponseGenerator
    {
        Task<Dictionary<string, object>> ProcessAsync(string subject, IEnumerable<string> scopes, Client client);
    }
}