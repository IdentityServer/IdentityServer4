// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Validation;

namespace IdentityServer4.Core.ResponseHandling
{
    public interface IIntrospectionResponseGenerator
    {
        Task<Dictionary<string, object>> ProcessAsync(IntrospectionRequestValidationResult validationResult, Scope scope);
    }
}