// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    /// <summary>
    /// Categories for events
    /// </summary>
    public static class EventCategories
    {
        /// <summary>
        /// Authentication related events
        /// </summary>
        public const string Authentication = "Authentication";

        /// <summary>
        /// Token related events
        /// </summary>
        public const string Token = "Token";

        /// <summary>
        /// Grants related events
        /// </summary>
        public const string Grants = "Grants";

        /// <summary>
        /// Error related events
        /// </summary>
        public const string Error = "Error";

        /// <summary>
        /// Device flow related events
        /// </summary>
        public const string DeviceFlow = "Device";
    }
}