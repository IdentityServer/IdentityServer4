// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNet.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Endpoints.Results
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

        public async Task ExecuteAsync(IdentityServerContext context)
        {
            var dto = new ErrorDto
            {
                error = Error,
                error_description = ErrorDescription
            };

            context.HttpContext.Response.StatusCode = 400;
            context.HttpContext.Response.SetNoCache();
            await context.HttpContext.Response.WriteJsonAsync(dto);
        }

        internal class ErrorDto
        {
            public string error { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string error_description { get; set; }
        }    
    }
}