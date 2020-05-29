// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Allows parsing raw scopes values into structured scope values.
    /// </summary>
    public interface IScopeParser
    {
        // todo: async?
        /// <summary>
        /// Parses the requested scopes.
        /// </summary>
        IEnumerable<ParsedScopeValue> ParseScopeValues(IEnumerable<string> scopeValues);
    }
}
