// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    /// <summary>
    /// Result for a discovery document
    /// </summary>
    /// <seealso cref="IdentityServer4.Hosting.IEndpointResult" />
    public class DiscoveryDocumentResult : IEndpointResult
    {
        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public Dictionary<string, object> Entries { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryDocumentResult"/> class.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <exception cref="System.ArgumentNullException">entries</exception>
        public DiscoveryDocumentResult(Dictionary<string, object> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            Entries = entries;
        }

        /// <summary>
        /// Executes the result.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public Task ExecuteAsync(HttpContext context)
        {
            var jobject = ObjectSerializer.ToJObject(Entries);
            return context.Response.WriteJsonAsync(jobject);
        }
    }
}