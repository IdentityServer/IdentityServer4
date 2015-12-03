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
using IdentityServer4.Core.Extensions;
using Newtonsoft.Json;

namespace IdentityServer4.Core.Results
{
    internal class TokenErrorResult : IResult
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