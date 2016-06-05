// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Hosting;
using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    interface IAuthorizeEndpointResultFactory
    {
        Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateLoginResultAsync(ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateConsentResultAsync(ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateAuthorizeResultAsync(ValidatedAuthorizeRequest request);
    }
}
