// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.ResponseHandling
{
    interface IAuthorizeInteractionResponseGenerator
    {
        Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null);
    }
}