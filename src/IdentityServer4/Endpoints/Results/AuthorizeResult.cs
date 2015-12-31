// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Endpoints.Results
{
    abstract class AuthorizeResult : IEndpointResult
    {
        public AuthorizeResponse Response { get; private set; }

        public AuthorizeResult(AuthorizeResponse response)
        {
            Response = response;
        }

        public abstract Task ExecuteAsync(IdentityServerContext context);
    }
}
