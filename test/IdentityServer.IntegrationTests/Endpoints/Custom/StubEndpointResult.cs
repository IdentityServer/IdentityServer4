// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;

namespace IdentityServer.IntegrationTests.Endpoints.Custom
{
    public class StubEndpointResult : IEndpointResult
    {
        public bool ExecuteAsyncWasCalled { get; set; }

        public Task ExecuteAsync(HttpContext context)
        {
            ExecuteAsyncWasCalled = true;
            return Task.CompletedTask;
        }
    }
}
