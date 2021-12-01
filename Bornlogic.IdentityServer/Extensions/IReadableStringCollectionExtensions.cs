// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.Extensions.Primitives;

#pragma warning disable 1591

namespace Bornlogic.IdentityServer.Extensions
{
    public static class IReadableStringCollectionExtensions
    {
        [DebuggerStepThrough]
        public static NameValueCollection AsNameValueCollection(this IEnumerable<KeyValuePair<string, StringValues>> collection)
        {
            var nv = new NameValueCollection();

            foreach (var field in collection)
            {
                nv.Add(field.Key, field.Value.First());
            }

            return nv;
        }

        [DebuggerStepThrough]
        public static NameValueCollection AsNameValueCollection(this IDictionary<string, StringValues> collection)
        {
            var nv = new NameValueCollection();

            foreach (var field in collection)
            {
                nv.Add(field.Key, field.Value.First());
            }

            return nv;
        }
    }
}