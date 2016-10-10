// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4
{
    public static class IdentityServerConstants
    {
        public const string DefaultCookieAuthenticationScheme = "idsvr";
        public const string SignoutScheme = "idsvr";
        public const string ExternalCookieAuthenticationScheme = "external";

        public static class ClaimValueTypes
        {
            public const string Json = "json";
        }

        public static class ParsedSecretTypes
        {
            public const string NoSecret = "NoSecret";
            public const string SharedSecret = "SharedSecret";
            public const string X509Certificate = "X509Certificate";
            public const string JwtBearer = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        }

        public static class SecretTypes
        {
            public const string SharedSecret = "SharedSecret";
            public const string X509CertificateThumbprint = "X509Thumbprint";
            public const string X509CertificateName = "X509Name";
            public const string X509CertificateBase64 = "X509CertificateBase64";
        }

        public static class ProfileDataCallers
        {
            public const string UserInfoEndpoint = "UserInfoEndpoint";
            public const string ClaimsProviderIdentityToken = "ClaimsProviderIdentityToken";
            public const string ClaimsProviderAccessToken = "ClaimsProviderAccessToken";
        }

        public static class ProfileIsActiveCallers
        {
            public const string AuthorizeEndpoint = "AuthorizeEndpoint";
            public const string IdentityTokenValidation = "IdentityTokenValidation";
            public const string AccessTokenValidation = "AccessTokenValidation";
            public const string ResourceOwnerValidation = "ResourceOwnerValidation";
            public const string RefreshTokenValidation = "RefreshTokenValidation";
            public const string AuthorizationCodeValidation = "AuthorizationCodeValidation";
        }
    }
}