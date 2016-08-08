﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Hosting
{
    public interface IEndpointRouter
    {
        // TODO: does this need to be async?
        IEndpoint Find(HttpContext context);
    }
}
