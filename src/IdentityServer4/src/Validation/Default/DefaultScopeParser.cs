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
        public IEnumerable<ParsedScopeValue> ParseScopeValues(IEnumerable<string> scopeValues)
        {
            if (scopeValues == null) throw new ArgumentNullException(nameof(scopeValues));

            var list = new List<ParsedScopeValue>();

            foreach (var scopeValue in scopeValues)
            {
                var parsedScopeValue = ParseScopeValue(scopeValue);
                if (parsedScopeValue != null)
                {
                    list.Add(parsedScopeValue);
                }
            }

            return list;
        }

        /// <summary>
        /// Parses a scope value.
        /// </summary>
        /// <param name="scopeValue"></param>
        /// <returns></returns>
        public virtual ParsedScopeValue ParseScopeValue(string scopeValue)
        {
            return new ParsedScopeValue(scopeValue);
        }
    }
}