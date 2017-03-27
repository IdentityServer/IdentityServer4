// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer4.ResponseHandling
{
    /// <summary>
    /// Interface for introspection response generator
    /// </summary>
    public interface IIntrospectionResponseGenerator
    {
        /// <summary>
        /// Processes the response.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="apiResource">The API resource.</param>
        /// <returns></returns>
        Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult, ApiResource apiResource);
    }
}