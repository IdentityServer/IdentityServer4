/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Microsoft.AspNet.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
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