// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace IdentityServer4.Core.Extensions
{
    internal static class DateTimeOffsetHelper
    {
        internal static Func<DateTimeOffset> UtcNowFunc = () => DateTimeOffset.UtcNow;

        internal static DateTimeOffset UtcNow
        {
            get
            {
                return UtcNowFunc();
            }
        }
    }
}
