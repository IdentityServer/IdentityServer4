// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Newtonsoft.Json;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using System;

namespace IdentityServer4.Endpoints.Results
{
    internal class TokenErrorResult : IEndpointResult
    {
        public TokenErrorResponse Error { get; }

        public TokenErrorResult(TokenErrorResponse error)
        {
            if (error.Error.IsMissing()) throw new ArgumentNullException(nameof(error.Error));

            Error = error;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            var dto = new ErrorDto
            {
                error = Error.Error,
                error_description = Error.ErrorDescription
            };

            context.Response.StatusCode = 400;
            context.Response.SetNoCache();
            await context.Response.WriteJsonAsync(dto);
        }

        internal class ErrorDto
        {
            public string error { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string error_description { get; set; }
        }    
    }
}