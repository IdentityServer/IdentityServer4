// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Bornlogic.IdentityServer.Validation.Models
{
    /// <summary>
    /// Models an error parsing a scope.
    /// </summary>
    public class ParsedScopeValidationError
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rawValue"></param>
        /// <param name="error"></param>
        public ParsedScopeValidationError(string rawValue, string error)
        {
            if (String.IsNullOrWhiteSpace(rawValue))
            {
                throw new ArgumentNullException(nameof(rawValue));
            }

            if (String.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentNullException(nameof(error));
            }

            RawValue = rawValue;
            Error = error;
        }

        /// <summary>
        /// The original (raw) value of the scope.
        /// </summary>
        public string RawValue { get; set; }

        /// <summary>
        /// Error message describing why the raw scope failed to be parsed.
        /// </summary>
        public string Error { get; set; }
    }
}