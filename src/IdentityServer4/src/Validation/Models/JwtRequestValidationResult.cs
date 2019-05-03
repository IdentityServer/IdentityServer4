// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace IdentityServer4.Validation
{
    internal class JwtRequestValidationResult : ValidationResult
    {
        public Dictionary<string, string> Payload { get; set; }
    }
}