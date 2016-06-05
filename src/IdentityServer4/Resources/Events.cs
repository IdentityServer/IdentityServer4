// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Reflection;

namespace IdentityServer4.Resources
{
    public static class Events
    {
        public static string GetString(string name)
        {
            return typeof(Events).GetField(name)?.GetValue(null)?.ToString();
        }

        public const string ClientPermissionsRevoked = "Client Permissions Revoked";
        public const string CspReport = "Content Security Policy(CSP) Report";
        public const string ExternalLoginError = "External Login Error";
        public const string ExternalLoginFailure = "External Login Failure";
        public const string ExternalLoginSuccess = "External Login Success";
        public const string LocalLoginFailure = "Local Login Failure";
        public const string LocalLoginSuccess = "Local Login Success";
        public const string LogoutEvent = "Logout Event";
        public const string PartialLogin = "Partial Login";
        public const string PartialLoginComplete = "Partial Login Complete";
        public const string PreLoginFailure = "Pre-Login Failure";
        public const string PreLoginSuccess = "Pre-Login Success";
        public const string ResourceOwnerFlowLoginFailure = "Resource Owner Password Flow Login Failure";
        public const string ResourceOwnerFlowLoginSuccess = "Resource Owner Password Flow Login Success";
    }
}
