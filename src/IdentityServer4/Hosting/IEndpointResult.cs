// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace IdentityServer4.Core.Hosting
{
    public interface IEndpointResult
    {
        Task ExecuteAsync(IdentityServerContext context);
    }
}