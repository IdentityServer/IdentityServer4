﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Validation
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
        /// <param name="api">The API.</param>
        /// <returns></returns>
        Task<IntrospectionRequestValidationResult> ValidateAsync(NameValueCollection parameters, ApiResource api);
    }
}