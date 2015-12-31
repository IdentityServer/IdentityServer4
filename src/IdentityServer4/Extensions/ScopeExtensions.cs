// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IdentityServer4.Core.Extensions
{
    internal static class ScopeExtensions
    {
        [DebuggerStepThrough]
        public static string ToSpaceSeparatedString(this IEnumerable<Scope> scopes)
        {
            var scopeNames = from s in scopes
                             select s.Name;

            return string.Join(" ", scopeNames.ToArray());
        }

        [DebuggerStepThrough]
        public static IEnumerable<string> ToStringList(this IEnumerable<Scope> scopes)
        {
            var scopeNames = from s in scopes
                             select s.Name;

            return scopeNames;
        }

        [DebuggerStepThrough]
        public static bool IncludesAllClaimsForUserRule(this IEnumerable<Scope> scopes, ScopeType type)
        {
            foreach (var scope in scopes)
            {
                if (scope.Type == type)
                {
                    if (scope.IncludeAllClaimsForUser)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}