// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using System.Security.Claims;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// Interface for the userinfo response generator
    /// </summary>
    public interface IUserInfoResponseGenerator
    {
        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="scopes">The scopes.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        Task<Dictionary<string, object>> ProcessAsync(ClaimsPrincipal subject, IEnumerable<string> scopes, Client client);
    }
}