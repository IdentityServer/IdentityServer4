// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Security.Claims;

namespace IdentityServer4.UnitTests.Common
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
