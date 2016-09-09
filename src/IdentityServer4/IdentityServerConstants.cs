// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4
{
    public static class IdentityServerConstants
    {
        public const string DefaultCookieAuthenticationScheme = "idsvr";
        public const string SignoutScheme = "idsvr";
        public const string ExternalCookieAuthenticationScheme = "external";

        public static class ClaimValueTypes
        {
            public const string Json = "json";
        }
    }
}