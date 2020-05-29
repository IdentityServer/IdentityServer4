// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Models a parsed scope value.
    /// </summary>
    public class ParsedScopeValue
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rawValue"></param>
        /// <param name="validationError"></param>
        public ParsedScopeValue(string rawValue, string validationError)
            : this(rawValue, rawValue, null)
        {
            if (String.IsNullOrWhiteSpace(validationError))
            {
                throw new ArgumentNullException(nameof(validationError));
            }

            ValidationError = validationError;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rawValue"></param>
        public ParsedScopeValue(string rawValue)
            : this(rawValue, rawValue, null)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rawValue"></param>
        /// <param name="parsedName"></param>
        /// <param name="parsedValue"></param>
        public ParsedScopeValue(string rawValue, string parsedName, string parsedValue)
        {
            if (String.IsNullOrWhiteSpace(rawValue))
            {
                throw new ArgumentNullException(nameof(rawValue));
            }
            if (String.IsNullOrWhiteSpace(parsedName))
            {
                throw new ArgumentNullException(nameof(parsedName));
            }

            RawValue = rawValue;
            ParsedName = parsedName;
            ParsedValue = parsedValue;
        }

        /// <summary>
        /// The original (raw) value of the scope.
        /// </summary>
        public string RawValue { get; set; }

        /// <summary>
        /// The parsed name of the scope. If the scope has no structure, the parsed name will be the same as the raw value.
        /// </summary>
        public string ParsedName { get; set; }

        // future: maybe this should be something w/ more structure? dictionary?
        /// <summary>
        /// The parsed value of the scope. If the scope has no structure, then the value will be null.
        /// </summary>
        public string ParsedValue { get; set; }

        /// <summary>
        /// Validation error message if the raw scope failed to be parsed.
        /// </summary>
        public string ValidationError { get; set; }

        /// <summary>
        /// Indicates if the parsed scope was successfully parsed.
        /// </summary>
        public bool IsValid => String.IsNullOrWhiteSpace(ValidationError);
    }
}