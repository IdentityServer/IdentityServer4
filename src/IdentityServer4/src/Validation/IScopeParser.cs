// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Allows parsing raw scopes values into structured scope values.
    /// </summary>
    public interface IScopeParser
    {
        // todo: async?
        // todo: test return no error, and no parsed scopes. how do callers behave?
        /// <summary>
        /// Parses the requested scopes.
        /// </summary>
        ParsedScopesResult ParseScopeValues(IEnumerable<string> scopeValues);
    }

    /// <summary>
    /// Represents the result of scope parsing.
    /// </summary>
    public class ParsedScopesResult
    {
        /// <summary>
        /// The valid parsed scopes.
        /// </summary>
        public ICollection<ParsedScopeValue> ParsedScopes { get; set; } = new HashSet<ParsedScopeValue>();

        /// <summary>
        /// The errors encountered while parsing.
        /// </summary>
        public ICollection<ParsedScopeValidationError> Errors { get; set; } = new HashSet<ParsedScopeValidationError>();
        
        /// <summary>
        /// Indicates if the result of parsing the scopes was successful.
        /// </summary>
        public bool Succeeded => Errors == null || !Errors.Any();
    }

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
