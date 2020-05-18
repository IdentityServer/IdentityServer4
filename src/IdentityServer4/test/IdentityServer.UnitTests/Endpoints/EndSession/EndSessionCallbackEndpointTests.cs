// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4.Endpoints;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IdentityServer.UnitTests.Endpoints.EndSession
{
    public class EndSessionCallbackEndpointTests
    {
        private const string Category = "End Session Callback Endpoint";

        StubEndSessionRequestValidator _stubEndSessionRequestValidator = new StubEndSessionRequestValidator();
        EndSessionCallbackEndpoint _subject;

        public EndSessionCallbackEndpointTests()
        {
            _subject = new EndSessionCallbackEndpoint(
                _stubEndSessionRequestValidator,
                new LoggerFactory().CreateLogger<EndSessionCallbackEndpoint>());
        }
    }
}
