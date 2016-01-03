// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.Http.Features.Authentication.Internal;
using System.Security.Claims;

namespace UnitTests.Common
{
    public static class HttpContextExtensions
    {
        public static void SetUser(this HttpContext context, ClaimsPrincipal user, string scheme = null)
        {
            var auth = context.GetAuthentication();
            auth.Handler = new StubAuthenticationHandler(user, scheme);
        }

        public static IHttpAuthenticationFeature GetAuthentication(this HttpContext context)
        {
            var auth = context.Features.Get<IHttpAuthenticationFeature>();
            if (auth == null)
            {
                auth = new HttpAuthenticationFeature();
                context.Features.Set(auth);
            }
            return auth;
        }
    }
}
