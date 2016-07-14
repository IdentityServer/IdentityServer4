// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using IdentityServer4.Extensions;

namespace IdentityServer4.Configuration
{
    public class UserInteractionOptions
    {
        public string LoginUrl { get; set; } = Constants.RoutePaths.Login.EnsureLeadingSlash();
        public string LoginReturnUrlParameter { get; set; } = Constants.RoutePathParams.Login;

        public string LogoutUrl { get; set; } = Constants.RoutePaths.Logout.EnsureLeadingSlash();
        public string LogoutReturnUrlParameter { get; set; } = Constants.RoutePathParams.Logout;

        public string ConsentUrl { get; set; } = Constants.RoutePaths.Consent.EnsureLeadingSlash();
        public string ConsentReturnUrlParameter { get; set; } = Constants.RoutePathParams.Consent;

        public string ErrorUrl { get; set; } = Constants.RoutePaths.Error.EnsureLeadingSlash();
        public string ErrorIdParameter { get; set; } = Constants.RoutePathParams.Error;
    }
}
