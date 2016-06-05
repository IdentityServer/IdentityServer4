// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Reflection;

namespace IdentityServer4.Resources
{
    public static class Scopes
    {
        public static string GetString(string name)
        {
            return typeof(Scopes).GetField(name)?.GetValue(null)?.ToString();
        }

        public const string address_DisplayName = "Your postal address";
        public const string all_claims_DisplayName = "All user information";
        public const string email_DisplayName = "Your email address";
        public const string offline_access_DisplayName = "Offline access";
        public const string openid_DisplayName = "Your user identifier";
        public const string phone_DisplayName   = "Your phone number";
        public const string profile_Description = "Your user profile information(first name, last name, etc.)";
        public const string profile_DisplayName = "User profile";
        public const string roles_DisplayName = "User roles";
    }
}
