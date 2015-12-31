// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Endpoints.Results
{
    public class LoginPageResult : RedirectToPageResult
    {
        public LoginPageResult(string id)
            : base(Constants.RoutePaths.Login, id)
        {
        }
    }
}
