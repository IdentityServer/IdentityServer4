// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Tests.Clients
{
    public class CustomGrantValidator2 : ICustomGrantValidator
    {
        public Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            var credential = request.Raw.Get("custom_credential");

            if (credential != null)
            {
                // valid credential
                return Task.FromResult(new CustomGrantValidationResult("818727", "custom"));
            }
            else
            {
                // custom error message
                return Task.FromResult(new CustomGrantValidationResult("invalid custom credential"));
            }
        }

        public string GrantType
        {
            get { return "custom2"; }
        }
    }
}