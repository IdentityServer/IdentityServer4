// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default implementation of IScopeParser.
    /// </summary>
    public class DefaultScopeParser : IScopeParser
    {
        /// <inheritdoc/>
        public ParsedScopesResult ParseScopeValues(IEnumerable<string> scopeValues)
        {
            if (scopeValues == null) throw new ArgumentNullException(nameof(scopeValues));

            var result = new ParsedScopesResult();

            foreach (var scopeValue in scopeValues)
            {
                var parsedScopeResult = ParseScopeValue(scopeValue);
                if (parsedScopeResult.Succeeded)
                {
                    result.ParsedScopes.Add(parsedScopeResult.ParsedScope);
                }
                else if (parsedScopeResult.Error != null)
                {
                    result.Errors.Add(parsedScopeResult.Error);
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a scope value.
        /// </summary>
        /// <param name="scopeValue"></param>
        /// <returns></returns>
        public virtual ParseScopeResult ParseScopeValue(string scopeValue)
        {
            return new ParseScopeResult(new ParsedScopeValue(scopeValue));
        }

        /// <summary>
        /// Models the result of parsing a scope.
        /// </summary>
        public class ParseScopeResult
        {
            /// <summary>
            /// Ctor
            /// </summary>
            public ParseScopeResult()
            {
            }

            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="parsedScope"></param>
            public ParseScopeResult(ParsedScopeValue parsedScope)
            {
                ParsedScope = parsedScope ?? throw new ArgumentNullException(nameof(parsedScope));
            }
            
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="error"></param>
            public ParseScopeResult(ParsedScopeValidationError error)
            {
                Error = error ?? throw new ArgumentNullException(nameof(error));
            }
            
            /// <summary>
            /// The parsed scopes.
            /// </summary>
            public ParsedScopeValue ParsedScope { get; set; }

            /// <summary>
            /// The error from parsing the scope.
            /// </summary>
            public ParsedScopeValidationError Error { get; set; }

            /// <summary>
            /// Indicates if parsing the scope was successful.
            /// </summary>
            public bool Succeeded => ParsedScope != null && Error == null;
        }
    }
}