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

        //////////////////////////////////////////////////////
        /// Client Authentication related events
        //////////////////////////////////////////////////////
        private const int ClientAuthenticationEventsStart = 2000;

        public const int ClientAuthenticationSuccess = ClientAuthenticationEventsStart + 1;
        public const int ClientAuthenticationFailure = ClientAuthenticationEventsStart + 2;
        
        //////////////////////////////////////////////////////
        /// API Authentication related events
        //////////////////////////////////////////////////////
        private const int ApiAuthenticationEventsStart = 3000;

        public const int ApiAuthenticationSuccess = ApiAuthenticationEventsStart + 1;
        public const int ApiAuthenticationFailure = ApiAuthenticationEventsStart + 2;

        //////////////////////////////////////////////////////
        /// Token related events
        //////////////////////////////////////////////////////
        private const int TokenEventsStart = 4000;

        public const int TokenIssuedSuccess = TokenEventsStart + 1;
        public const int TokenIssuedFailure = TokenEventsStart + 2;

        public const int TokenRevokedSuccess = TokenEventsStart + 3;
    }
}