// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#pragma warning disable 1591

using System;

namespace IdentityServer4
{
    public static class IdentityServerDateTime
    {
        public static DateTime UtcNow => UtcNowFunc();
        public static Func<DateTime> UtcNowFunc = () => DateTime.UtcNow;
    }
}
