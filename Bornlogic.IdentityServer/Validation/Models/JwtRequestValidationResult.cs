// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Bornlogic.IdentityServer.Validation.Models
{
    /// <summary>
    /// Models the result of JWT request validation.
    /// </summary>
    public class JwtRequestValidationResult : ValidationResult
    {
        /// <summary>
        /// The key/value pairs from the JWT payload of a successfuly validated request.
        /// </summary>
        public Dictionary<string, string> Payload { get; set; }
    }
}