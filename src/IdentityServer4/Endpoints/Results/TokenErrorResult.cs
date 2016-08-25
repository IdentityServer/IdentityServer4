// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNet.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Endpoints.Results
{
    internal class TokenErrorResult : IEndpointResult
    {
        public string Error { get; internal set; }
        public string ErrorDescription { get; internal set; }

        public TokenErrorResult(string error)
        {
            Error = error;
        }

        public TokenErrorResult(string error, string errorDescription)
        {
            Error = error;
            ErrorDescription = errorDescription;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            var dto = new ErrorDto
            {
                error = Error,
                error_description = ErrorDescription
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