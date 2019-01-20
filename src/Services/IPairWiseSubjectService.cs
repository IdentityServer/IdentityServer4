// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer4.Services
{
    /// <summary>
    /// This interface allows IdentityServer to create pair wise subject.
    /// </summary>
    public interface IPairWiseSubjectService
    {
        /// <summary>
        /// Returns pair wise subject for a user
        /// </summary>
        /// <param name="subject">The original subject string.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        string GetPairWiseSubject(string subject, Client client);
    }
}