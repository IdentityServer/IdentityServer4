// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;

namespace IdentityServer4.Endpoints.Results
{
    /// <summary>
    /// Result for introspection
    /// </summary>
    /// <seealso cref="IdentityServer4.Hosting.IEndpointResult" />
    public class IntrospectionResult : IEndpointResult
    {
        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public Dictionary<string, object> Result { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectionResult"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <exception cref="System.ArgumentNullException">result</exception>
        public IntrospectionResult(Dictionary<string, object> result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>
        /// Executes the result.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        public Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();
            return context.Response.WriteJsonAsync(Result);
        }
    }
}