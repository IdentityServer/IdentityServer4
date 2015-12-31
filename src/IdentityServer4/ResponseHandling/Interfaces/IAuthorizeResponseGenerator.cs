// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using IdentityServer4.Core.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    interface IAuthorizeResponseGenerator
    {
        Task<AuthorizeResponse> CreateResponseAsync(ValidatedAuthorizeRequest request);
    }
}