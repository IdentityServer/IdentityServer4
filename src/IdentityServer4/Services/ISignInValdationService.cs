// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// The claims service is responsible for determining which claims to include in tokens
    /// </summary>
    public interface ISignInValdationService
    {
        Task ValidateAsync(SignInContext context);
   }
}