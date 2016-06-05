// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Reflection;

namespace IdentityServer4.Resources
{
    public static class Messages
    {
        public static string GetString(string name)
        {
            return typeof(Messages).GetField(name)?.GetValue(null)?.ToString();
        }

        public const string ClientIdRequired = "Client identifier is required";
        public const string ExternalProviderError = "There was an error logging into the external provider.The error message is: {0}";
        public const string invalid_request = "The client application made an invalid request.";
        public const string invalid_scope = "The client application tried to access a resource it does not have access to.";
        public const string InvalidUsernameOrPassword = "Invalid username or password";
        public const string MissingClientId = "client_id is missing";
        public const string MissingToken = "Token is missing";
        public const string NoExternalProvider = "The external login provider was not provided.";
        public const string NoMatchingExternalAccount = "Invalid Account";
        public const string NoSignInCookie = "There is an error determining which application you are signing into.Return to the application and try again.";
        public const string NoSubjectFromExternalProvider = "Error authenticating with external provider";
        public const string PasswordRequired = "Password is required";
        public const string SslRequired = "SSL is required";
        public const string unauthorized_client = "The client application is not known or is not authorized.";
        public const string UnexpectedError = "There was an unexpected error";
        public const string unsupported_response_type = "The authorization server does not support the requested response type.";
        public const string UnsupportedMediaType = "Unsupported Media Type";
        public const string UsernameRequired = "Username is required";
    }
}
