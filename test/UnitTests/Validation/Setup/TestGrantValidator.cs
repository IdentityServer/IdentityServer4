// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Tests.Validation
{
    class TestGrantValidator : ICustomGrantValidator
    {
        public Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            return Task.FromResult(new CustomGrantValidationResult("bob", "CustomGrant"));
        }

        public string GrantType
        {
            get { return "custom_grant"; }
        }
    }
}