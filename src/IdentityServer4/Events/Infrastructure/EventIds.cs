// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    public static class EventIds
    {
        //////////////////////////////////////////////////////
        /// User Authentication related events
        //////////////////////////////////////////////////////
        private const int UserAuthenticationEventsStart = 1000;

        public const int UserLoginSuccess = UserAuthenticationEventsStart + 1;
        public const int UserLoginFailure = UserAuthenticationEventsStart + 2;

        public const int UserLogoutSuccess = UserAuthenticationEventsStart + 3;
    }
}