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
                var ctx = new ParseScopeContext(scopeValue);
                ParseScopeValue(ctx);
                
                if (ctx.Succeeded)
                {
                    var parsedScope = ctx.ParsedName != null ?
                        new ParsedScopeValue(ctx.RawValue, ctx.ParsedName, ctx.ParameterValue) :
                        new ParsedScopeValue(ctx.RawValue);

                    result.ParsedScopes.Add(parsedScope);
                }
                else if (!ctx.Ignore)
                {
                    result.Errors.Add(new ParsedScopeValidationError(scopeValue, ctx.Error));
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a scope value.
        /// </summary>
        /// <param name="scopeContext"></param>
        /// <returns></returns>
        public virtual void ParseScopeValue(ParseScopeContext scopeContext)
        {
            // nop leaves the raw scope value as a success result.
        }

        /// <summary>
        /// Models the context for parsing a scope.
        /// </summary>
        public class ParseScopeContext
        {
            /// <summary>
            /// The original (raw) value of the scope.
            /// </summary>
            public string RawValue { get; }

            /// <summary>
            /// The parsed name of the scope. If the scope has no structure, the parsed name will be the same as the raw value.
            /// </summary>
            public string ParsedName { get; private set; }

            /// <summary>
            /// The parameter value of the parsed scope. If the scope has no structure, then the value will be null.
            /// </summary>
            public string ParameterValue { get; private set; }

            /// <summary>
            /// The error encountered parsing the scope.
            /// </summary>
            public string Error { get; private set; }
            
            /// <summary>
            /// Indicates if the scope should be excluded from the parsed results.
            /// </summary>
            public bool Ignore { get; private set; }

            /// <summary>
            /// Indicates if parsing the scope was successful.
            /// </summary>
            public bool Succeeded => !Ignore && Error == null;


            /// <summary>
            /// Ctor. Indicates success, but the scope should not be included in result.
            /// </summary>
            internal ParseScopeContext(string rawScopeValue)
            {
                RawValue = rawScopeValue;
            }

            /// <summary>
            /// Sets the parsed name and optional parsed parameter value for the scope.
            /// </summary>
            /// <param name="parsedName"></param>
            /// <param name="parameterValue"></param>
            public void SetParsedValues(string parsedName, string parameterValue = null)
            {
                if (String.IsNullOrWhiteSpace(parsedName))
                {
                    throw new ArgumentNullException(nameof(parsedName));
                }

                ParsedName = parsedName;
                ParameterValue = parameterValue;
                Error = null;
                Ignore = false;
            }

            /// <summary>
            /// Set the error encountered parsing the scope.
            /// </summary>
            /// <param name="error"></param>
            public void SetError(string error)
            {
                ParsedName = null;
                ParameterValue = null;
                Error = error;
                Ignore = false;
            }

            /// <summary>
            /// Sets that the scope is to be ignore/excluded from the parsed results.
            /// </summary>
            public void SetIgnore()
            {
                ParsedName = null;
                ParameterValue = null;
                Error = null;
                Ignore = true;
            }
        }
    }
}