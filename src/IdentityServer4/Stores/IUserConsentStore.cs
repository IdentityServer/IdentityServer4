﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// Implements user consent storage
    /// </summary>
    public interface IUserConsentStore
    {
        Task StoreUserConsentAsync(Consent consent);
        Task<Consent> GetUserConsentAsync(string subjectId, string clientId);
        Task RemoveUserConsentAsync(string subjectId, string clientId);
    }
}