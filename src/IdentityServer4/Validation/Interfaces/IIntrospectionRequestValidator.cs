// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Interface for the introspection request validator
    /// </summary>
    public interface IIntrospectionRequestValidator
    {
        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="apiResource">The API resource.</param>
        /// <returns></returns>
        Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, ApiResource apiResource);
    }
}