// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#pragma warning disable 1591

namespace IdentityServer4.Models
{
    public static class GrantType
    {
        public const string Implicit = "implicit";
        public const string Hybrid = "hybrid";
        public const string AuthorizationCode = "authorization_code";
        public const string ClientCredentials = "client_credentials";

        /// <summary>
        /// As Identity Server does not know how to validate user password you need to implement and register the <see cref="IdentityServer4.Validation.IResourceOwnerPasswordValidator"/> interface.
        /// </summary>
        public const string ResourceOwnerPassword = "password";
        public const string DeviceFlow = "urn:ietf:params:oauth:grant-type:device_code";
    }
}