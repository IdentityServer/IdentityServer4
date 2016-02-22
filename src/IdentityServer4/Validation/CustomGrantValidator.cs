// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Validation
{
    public class CustomGrantValidator
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<ICustomGrantValidator> _validators;
        
        public CustomGrantValidator(IEnumerable<ICustomGrantValidator> validators, ILogger<CustomGrantValidator> logger)
        {
            if (validators == null)
            {
                _validators = Enumerable.Empty<ICustomGrantValidator>();
            }
            else
            {
                _validators = validators;
            }

            _logger = logger;
        }

        public IEnumerable<string> GetAvailableGrantTypes()
        {
            return _validators.Select(v => v.GrantType);
        }

        public async Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            var validator = _validators.FirstOrDefault(v => v.GrantType.Equals(request.GrantType, StringComparison.Ordinal));

            if (validator == null)
            {
                return new CustomGrantValidationResult("No validator found for grant type");
            }

            try
            {
                return await validator.ValidateAsync(request);
            }
            catch (Exception e)
            {
                _logger.LogError("Grant validation error", e);
                return new CustomGrantValidationResult("Grant validation error");
            }
        }
    }
}