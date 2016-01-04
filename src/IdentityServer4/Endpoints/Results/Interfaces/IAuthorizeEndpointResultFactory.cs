// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints.Results
{
    interface IAuthorizeEndpointResultFactory
    {
        Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateLoginResultAsync(ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateConsentResultAsync(ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateAuthorizeResultAsync(ValidatedAuthorizeRequest request);
    }
}
