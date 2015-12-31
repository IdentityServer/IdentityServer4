// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;

namespace IdentityServer4.Core.Extensions
{
    internal static class DateTimeOffsetExtensions
    {
        [DebuggerStepThrough]
        public static bool HasExceeded(this DateTimeOffset creationTime, int seconds)
        {
            return (DateTimeOffsetHelper.UtcNow > creationTime.AddSeconds(seconds));
        }

        [DebuggerStepThrough]
        public static int GetLifetimeInSeconds(this DateTimeOffset creationTime)
        {
            return ((int)(DateTimeOffsetHelper.UtcNow - creationTime).TotalSeconds);
        }

        [DebuggerStepThrough]
        public static bool HasExpired(this DateTimeOffset? expirationTime)
        {
            if (expirationTime.HasValue &&
                expirationTime < DateTimeOffsetHelper.UtcNow)
            {
                return true;
            }

            return false;
        }
    }
}