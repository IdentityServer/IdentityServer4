﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Events
{
    public static class EventConstants
    {
        public static class Categories
        {
            public const string Authentication = "Authentication";
            public const string ClientAuthentication = "ClientAuthentication";
            public const string TokenService = "TokenService";
            public const string Endpoints = "Endpoints";
            public const string Information = "Information";
            public const string InternalError = "InternalError";
        }

        public static class EndpointNames
        {
            public const string Authenticate = "authenticate";
            public const string Authorize = "authorize";
            public const string Token = "token";
            public const string Revocation = "revocation";
            public const string UserInfo = "userinfo";
            public const string EndSession = "endsession";
            public const string AccessTokenValidation = "accesstokenvalidation";
            public const string Introspection = "introspection";
            public const string IdentityTokenValidation = "identitytokenvalidaton";
            public const string CspReport = "cspreport";
            public const string ClientPermissions = "clientpermissions";
        }

        public static class ClientTypes
        {
            public const string Client = "Client";
            public const string Scope = "Scope";
        }
        
        public static class Ids
        {
            ///////////////////////////
            /// Authentication related events
            ///////////////////////////
            private const int AuthenticationEventsStart = 1000;

            public const int PreLoginSuccess = AuthenticationEventsStart + 0;
            public const int PreLoginFailure = AuthenticationEventsStart + 1;

            public const int LocalLoginSuccess = AuthenticationEventsStart + 10;
            public const int LocalLoginFailure = AuthenticationEventsStart + 11;

            public const int ExternalLoginSuccess = AuthenticationEventsStart + 20;
            public const int ExternalLoginFailure = AuthenticationEventsStart + 21;
            public const int ExternalLoginError = AuthenticationEventsStart + 22;
            
            public const int Logout = AuthenticationEventsStart + 30;

            public const int PartialLogin = AuthenticationEventsStart + 40;
            public const int PartialLoginComplete = AuthenticationEventsStart + 41;

            public const int ResourceOwnerFlowLoginSuccess = AuthenticationEventsStart + 50;
            public const int ResourceOwnerFlowLoginFailure = AuthenticationEventsStart + 51;

            public const int ClientAuthenticationSuccess = AuthenticationEventsStart + 60;
            public const int ClientAuthenticationFailure = AuthenticationEventsStart + 51;

            ///////////////////////////
            /// Token service related events
            ///////////////////////////
            private const int TokenServiceEventsStart = 2000;

            public const int AccessTokenIssued = TokenServiceEventsStart + 0;
            public const int IdentityTokenIssued = TokenServiceEventsStart + 1;

            public const int AuthorizationCodeIssued = TokenServiceEventsStart + 10;
            public const int AuthorizationCodeRedeemedSuccess = TokenServiceEventsStart + 11;
            public const int AuthorizationCodeRedeemedFailure = TokenServiceEventsStart + 12;

            public const int RefreshTokenIssued = TokenServiceEventsStart + 20;
            public const int RefreshTokenRefreshedSuccess = TokenServiceEventsStart + 21;
            public const int RefreshTokenRefreshedFailure = TokenServiceEventsStart + 22;

            public const int PermissionRevoked = TokenServiceEventsStart + 30;
            
            
            ///////////////////////////
            /// Endpoints related events
            ///////////////////////////
            private const int EndpointsEventsStart = 3000;

            public const int EndpointSuccess = EndpointsEventsStart + 0;
            public const int EndpointFailure = EndpointsEventsStart + 1;

            public const int IntrospectionEndpointSuccess = EndpointsEventsStart + 5;
            public const int IntrospectionEndpointFailure = EndpointsEventsStart + 6;

            ///////////////////////////
            /// Information events
            ///////////////////////////
            private const int InformationEventsStart = 4000;

            public const int CertificateExpiration = InformationEventsStart + 0;
            public const int CspReport = InformationEventsStart + 1;
            public const int ClientPermissionRevoked = InformationEventsStart + 2;

            public const int NoSigningCertificateConfigured = InformationEventsStart + 10;
            public const int SigningCertificateExpiringSoon = InformationEventsStart + 11;
            public const int SigningCertificateValidated = InformationEventsStart + 12;


            ///////////////////////////
            /// Error events
            ///////////////////////////
            private const int InternalErrorEventsStart = 5000;

            public const int UnhandledExceptionError = InternalErrorEventsStart + 0;
            public const int SigningCertificatePrivateKeyNotAccessible = InternalErrorEventsStart + 1;
        }
    }
}