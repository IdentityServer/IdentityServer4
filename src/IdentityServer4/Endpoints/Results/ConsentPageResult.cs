// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;

namespace IdentityServer4.Endpoints.Results
{
    public class ConsentPageResult : RedirectToPageWithReturnUrlResult
    {
        public ConsentPageResult(UserInteractionOptions options, string returnUrl)
            : base(options.ConsentUrl, options.ConsentReturnUrlParameter, returnUrl)
        {
        }
    }
}
