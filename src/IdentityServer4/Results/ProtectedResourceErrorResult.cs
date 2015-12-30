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

using System.Threading.Tasks;
using IdentityServer4.Core.Extensions;
using Microsoft.Extensions.Primitives;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    internal class ProtectedResourceErrorResult : IEndpointResult
    {
        public string Error;
        public string ErrorDescription;

        public ProtectedResourceErrorResult(string error, string errorDescription = null)
        {
            Error = error;
            ErrorDescription = errorDescription;
        }

        public Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.StatusCode = 401;

            if (Constants.ProtectedResourceErrorStatusCodes.ContainsKey(Error))
            {
                context.HttpContext.Response.StatusCode = Constants.ProtectedResourceErrorStatusCodes[Error];
            }

            var parameter = string.Format("error=\"{0}\"", Error);
            if (ErrorDescription.IsPresent())
            {
                parameter = string.Format("{0}, error_description=\"{1}\"",
                    parameter, ErrorDescription);
            }

            context.HttpContext.Response.Headers.Add("WwwAuthentication", new StringValues("Bearer"));
            //TODO logger.LogInformation("Returning error: " + Error);

            return Task.FromResult(0);
        }
    }
}