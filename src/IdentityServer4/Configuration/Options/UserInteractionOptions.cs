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
        public string LoginUrl { get; set; } = Constants.UIConstants.DefaultRoutePaths.Login.EnsureLeadingSlash();
        public string LoginReturnUrlParameter { get; set; } = Constants.UIConstants.DefaultRoutePathParams.Login;

        public string LogoutUrl { get; set; } = Constants.UIConstants.DefaultRoutePaths.Logout.EnsureLeadingSlash();
        public string LogoutIdParameter { get; set; } = Constants.UIConstants.DefaultRoutePathParams.Logout;

        public string ConsentUrl { get; set; } = Constants.UIConstants.DefaultRoutePaths.Consent.EnsureLeadingSlash();
        public string ConsentReturnUrlParameter { get; set; } = Constants.UIConstants.DefaultRoutePathParams.Consent;

        public string ErrorUrl { get; set; } = Constants.UIConstants.DefaultRoutePaths.Error.EnsureLeadingSlash();
        public string ErrorIdParameter { get; set; } = Constants.UIConstants.DefaultRoutePathParams.Error;

        public int CookieMessageThreshold { get; set; } = Constants.UIConstants.CookieMessageThreshold;
    }
}
