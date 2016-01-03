// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Core.Extensions;
using Microsoft.Extensions.Primitives;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http;

namespace IdentityServer4.Core.Endpoints.Results
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
            context.HttpContext.Response.SetNoCache();

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