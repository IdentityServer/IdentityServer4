// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Endpoints.Results
{
    public class ConsentPageResult : RedirectToPageResult
    {
        public ConsentPageResult(string id)
            : base(Constants.RoutePaths.Consent, id)
        {
        }
    }
}
