// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Configuration;

namespace IdentityServer4.Endpoints.Results
{
    public class ErrorPageResult : RedirectToPageResult
    {
        public ErrorPageResult(UserInteractionOptions options, string id)
            : base(options.ErrorUrl, options.ErrorIdParameter, id)
        {
        }
    }
}
