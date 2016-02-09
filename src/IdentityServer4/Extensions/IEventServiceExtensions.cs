// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Events;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Extensions
{
    internal static class IEventServiceExtensions
    {
        public static async Task RaisePreLoginSuccessEventAsync(this IEventService events, 
            string signInMessageId, SignInRequest signInRequest, AuthenticateResult authResult)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.Authentication,
                "Pre-Login Success",
                EventTypes.Success, 
                EventConstants.Ids.PreLoginSuccess,
                new LoginDetails {
                    SubjectId = authResult.HasSubject ?  authResult.User.GetSubjectId() : null,
                    Name = authResult.User.Identity.Name,
                    SignInId = signInMessageId,
                    SignInRequest = signInRequest,
                    PartialLogin = authResult.IsPartialSignIn
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaisePreLoginFailureEventAsync(this IEventService events, 
            string signInMessageId, SignInRequest signInRequest, string error)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.Authentication,
                "Pre-Login Failure",
                EventTypes.Failure,
                EventConstants.Ids.PreLoginFailure,
                new LoginDetails
                {
                    SignInId = signInMessageId,
                    SignInRequest = signInRequest,
                }, 
                error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseLocalLoginSuccessEventAsync(this IEventService events, 
            string username, string signInMessageId, SignInRequest signInRequest, AuthenticateResult authResult)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.Authentication,
                "Local Login Success",
                EventTypes.Success,
                EventConstants.Ids.LocalLoginSuccess,
                new LocalLoginDetails
                {
                    SubjectId = authResult.HasSubject ? authResult.User.GetSubjectId() : null,
                    Name = authResult.User.Identity.Name,
                    SignInId = signInMessageId,
                    SignInRequest = signInRequest,
                    PartialLogin = authResult.IsPartialSignIn,
                    LoginUserName = username
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseLocalLoginFailureEventAsync(this IEventService events, 
            string username, string signInMessageId, SignInRequest signInRequest, string error)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.Authentication,
                "Local Login Failure",
                EventTypes.Failure,
                EventConstants.Ids.LocalLoginFailure,
                new LocalLoginDetails
                {
                    SignInId = signInMessageId,
                    SignInRequest = signInRequest,
                    LoginUserName = username
                }, 
                error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseExternalLoginSuccessEventAsync(this IEventService events, 
            ExternalIdentity externalIdentity, string signInMessageId, SignInRequest signInRequest, AuthenticateResult authResult)
        {
            var evt = new Event<ExternalLoginDetails>(
                EventConstants.Categories.Authentication,
                "External Login Success",
                EventTypes.Success,
                EventConstants.Ids.ExternalLoginSuccess,
                new ExternalLoginDetails
                {
                    SubjectId = authResult.HasSubject ? authResult.User.GetSubjectId() : null,
                    Name = authResult.User.Identity.Name,
                    SignInId = signInMessageId,
                    SignInRequest = signInRequest,
                    PartialLogin = authResult.IsPartialSignIn,
                    Provider = externalIdentity.Provider,
                    ProviderId = externalIdentity.ProviderId,
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseExternalLoginFailureEventAsync(this IEventService events, 
            ExternalIdentity externalIdentity, string signInMessageId, SignInRequest signInRequest, string error)
        {
            var evt = new Event<ExternalLoginDetails>(
                EventConstants.Categories.Authentication,
                "External Login Failure",
                EventTypes.Failure,
                EventConstants.Ids.ExternalLoginFailure,
                new ExternalLoginDetails
                {
                    SignInId = signInMessageId,
                    SignInRequest = signInRequest,
                    Provider = externalIdentity.Provider,
                    ProviderId = externalIdentity.ProviderId,
                }, 
                error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseExternalLoginErrorEventAsync(this IEventService events, string error)
        {
            var evt = new Event<object>(
               EventConstants.Categories.Authentication,
               "External Login Error",
               EventTypes.Error,
               EventConstants.Ids.ExternalLoginError,
               error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseSuccessfulResourceOwnerFlowAuthenticationEventAsync(this IEventService events, 
            string userName, string subjectId)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.Authentication,
                "Resource Owner Flow Login Success",
                EventTypes.Success,
                EventConstants.Ids.ResourceOwnerFlowLoginSuccess,
                new LocalLoginDetails
                {
                    SubjectId = subjectId,
                    LoginUserName = userName
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseFailedResourceOwnerFlowAuthenticationEventAsync(this IEventService events, 
            string userName, string error)
        {
            var evt = new Event<LocalLoginDetails>(
                EventConstants.Categories.Authentication,
                "Resource Owner Flow Login Failure",
                EventTypes.Failure,
                EventConstants.Ids.ResourceOwnerFlowLoginFailure,
                new LocalLoginDetails
                {
                    LoginUserName = userName
                },
                error);

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaisePartialLoginCompleteEventAsync(this IEventService events, 
            ClaimsIdentity subject, string signInMessageId, SignInRequest signInRequest)
        {
            var evt = new Event<LoginDetails>(
                EventConstants.Categories.Authentication,
                "Partial Login Complete",
                EventTypes.Information,
                EventConstants.Ids.PartialLoginComplete,
                new LoginDetails
                {
                    SubjectId = subject.GetSubjectId(),
                    Name = subject.Name,
                    SignInId = signInMessageId,
                    SignInRequest = signInRequest
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseLogoutEventAsync(this IEventService events, 
            ClaimsPrincipal subject, string signOutId, SignOutRequest signOutMessage)
        {
            var evt = new Event<LogoutDetails>(
                EventConstants.Categories.Authentication,
                "Logout",
                EventTypes.Information,
                EventConstants.Ids.Logout,
                new LogoutDetails
                {
                    SubjectId = subject.GetSubjectId(),
                    Name = subject.Identity.Name,
                    SignOutId = signOutId,
                    SignOutMessage = signOutMessage
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseCspReportEventAsync(this IEventService events, string report, ClaimsPrincipal user)
        {
            var evt = new Event<CspReportDetails>(
                EventConstants.Categories.Information,
                "Content Security Policy (CSP) Report",
                EventTypes.Information,
                EventConstants.Ids.CspReport);

            evt.DetailsFunc = () => {
                string subject = null;
                string name = null;
                if (user != null && user.Identity.IsAuthenticated)
                {
                    subject = user.GetSubjectId();
                    name = user.Identity.Name;
                }

                object reportData = null;
                try
                {
                    reportData = Newtonsoft.Json.JsonConvert.DeserializeObject(report);
                }
                catch(Newtonsoft.Json.JsonReaderException)
                {
                    reportData = "Error reading CSP report JSON";
                    evt.Message = "Raw Report Data: " + report;
                }
                return new CspReportDetails
                {
                    Subject = subject,
                    Name = name,
                    Report = reportData
                };
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseClientPermissionsRevokedEventAsync(this IEventService events, ClaimsPrincipal user, string clientId)
        {
            var evt = new Event<ClientPermissionsRevokedDetails>(
                EventConstants.Categories.Information,
                "Client Permissions Revoked",
                EventTypes.Information,
                EventConstants.Ids.ClientPermissionRevoked,
                new ClientPermissionsRevokedDetails
                {
                    Subject = user.GetSubjectId(),
                    Name = user.Identity.Name,
                    ClientId = clientId
                });

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseTokenIssuedEventAsync(this IEventService events, Token token, string rawToken)
        {
            if (token.Type == OidcConstants.TokenTypes.AccessToken)
            {
                await events.RaiseAccessTokenIssuedEventAsync(token, rawToken);
            }
            else if (token.Type == OidcConstants.TokenTypes.IdentityToken)
            {
                await events.RaiseIdentityTokenIssuedEventAsync(token);
            }
        }

        public static async Task RaiseAccessTokenIssuedEventAsync(this IEventService events, Token token, string rawToken)
        {
            var evt = new Event<AccessTokenIssuedDetails>(
                EventConstants.Categories.TokenService,
                "Access token issued",
                EventTypes.Information,
                EventConstants.Ids.AccessTokenIssued);

            string referenceTokenHandle = null;
            if (token.Client.AccessTokenType == AccessTokenType.Reference)
            {
                referenceTokenHandle = rawToken;
            }

            evt.DetailsFunc = () => new AccessTokenIssuedDetails
            {
                SubjectId = token.SubjectId ?? "no subject id",
                ClientId = token.ClientId,
                TokenType = token.Client.AccessTokenType,
                Lifetime = token.Lifetime,
                Scopes = token.Scopes,
                Claims = token.Claims.ToClaimsDictionary(),
                ReferenceTokenHandle = referenceTokenHandle
            };

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseIdentityTokenIssuedEventAsync(this IEventService events, Token token)
        {
            var evt = new Event<TokenIssuedDetailsBase>(
                EventConstants.Categories.TokenService,
                "Identity token issued",
                EventTypes.Information,
                EventConstants.Ids.IdentityTokenIssued);

            evt.DetailsFunc = () => new TokenIssuedDetailsBase
            {
                SubjectId = token.SubjectId,
                ClientId = token.ClientId,
                Lifetime = token.Lifetime,
                Claims = token.Claims.ToClaimsDictionary()
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseAuthorizationCodeIssuedEventAsync(this IEventService events, string id, AuthorizationCode code)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TokenService,
                "Authorization code issued",
                EventTypes.Information,
                EventConstants.Ids.AuthorizationCodeIssued);

            evt.DetailsFunc = () => new AuthorizationCodeDetails
            {
                HandleId = id,
                ClientId = code.ClientId,
                Scopes = code.Scopes,
                SubjectId = code.SubjectId,
                RedirectUri = code.RedirectUri,
                Lifetime = code.Client.AuthorizationCodeLifetime
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseRefreshTokenIssuedEventAsync(this IEventService events, string id, RefreshToken token)
        {
            var evt = new Event<RefreshTokenDetails>(
                EventConstants.Categories.TokenService,
                "Refresh token issued",
                EventTypes.Information,
                EventConstants.Ids.RefreshTokenIssued);

            evt.DetailsFunc = () => new RefreshTokenDetails
            {
                HandleId = id,
                ClientId = token.ClientId,
                Scopes = token.Scopes,
                SubjectId = token.SubjectId,
                Lifetime = token.LifeTime,
                Version = token.Version
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseSuccessfulRefreshTokenRefreshEventAsync(this IEventService events, string oldHandle, string newHandle, RefreshToken token)
        {
            var evt = new Event<RefreshTokenRefreshDetails>(
                EventConstants.Categories.TokenService,
                "Refresh token refresh success",
                EventTypes.Success,
                EventConstants.Ids.RefreshTokenRefreshedSuccess);

            evt.Details = new RefreshTokenRefreshDetails
            {
                OldHandle = oldHandle,
                NewHandle = newHandle,
                ClientId = token.ClientId,
                Lifetime = token.LifeTime
            };

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseUnhandledExceptionEventAsync(this IEventService events, Exception exception)
        {
            var evt = new Event<object>(
                EventConstants.Categories.InternalError,
                "Unhandled exception",
                EventTypes.Error,
                EventConstants.Ids.UnhandledExceptionError, 
                exception.ToString());

            await events.RaiseEventAsync(evt);
        }

        public static async Task RaiseSuccessfulClientAuthenticationEventAsync(this IEventService events, string clientId, string clientType)
        {
            var evt = new Event<ClientAuthenticationDetails>(
                EventConstants.Categories.ClientAuthentication,
                "Client authentication success",
                EventTypes.Success,
                EventConstants.Ids.ClientAuthenticationSuccess,
                new ClientAuthenticationDetails
                {
                    ClientId = clientId,
                    ClientType = clientType
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailureClientAuthenticationEventAsync(this IEventService events, string message, string clientId, string clientType)
        {
            var evt = new Event<ClientAuthenticationDetails>(
                EventConstants.Categories.ClientAuthentication,
                "Client authentication failure",
                EventTypes.Failure,
                EventConstants.Ids.ClientAuthenticationFailure,
                new ClientAuthenticationDetails
                {
                    ClientId = clientId,
                    ClientType = clientType
                },
                message);

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseSuccessfulEndpointEventAsync(this IEventService events, string endpointName)
        {
            var evt = new Event<EndpointDetail>(
                EventConstants.Categories.Endpoints,
                "Endpoint success",
                EventTypes.Success,
                EventConstants.Ids.EndpointSuccess,
                new EndpointDetail { EndpointName = endpointName });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailureEndpointEventAsync(this IEventService events, string endpointName, string error)
        {
            var evt = new Event<EndpointDetail>(
                 EventConstants.Categories.Endpoints,
                 "Endpoint failure",
                 EventTypes.Failure,
                 EventConstants.Ids.EndpointFailure,
                 new EndpointDetail { EndpointName = endpointName },
                 error);

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseSuccessfulIntrospectionEndpointEventAsync(this IEventService events, string token, string tokenStatus, string scopeName)
        {
            var evt = new Event<IntrospectionEndpointDetail>(
                EventConstants.Categories.Endpoints,
                "Introspection endpoint success",
                EventTypes.Success,
                EventConstants.Ids.IntrospectionEndpointSuccess,
                new IntrospectionEndpointDetail
                {
                    Token = token,
                    TokenStatus = tokenStatus,
                    ScopeName = scopeName
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailureIntrospectionEndpointEventAsync(this IEventService events, string error, string token, string scopeName)
        {
            var evt = new Event<IntrospectionEndpointDetail>(
                 EventConstants.Categories.Endpoints,
                 "Introspection endpoint failure",
                 EventTypes.Failure,
                 EventConstants.Ids.IntrospectionEndpointFailure,
                 new IntrospectionEndpointDetail
                 {
                     Token = token,
                     ScopeName = scopeName
                 },
                 error);

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailedAuthorizationCodeRedeemedEventAsync(this IEventService events, Client client, string handle, string error)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TokenService,
                "Authorization code redeem failure",
                EventTypes.Failure,
                EventConstants.Ids.AuthorizationCodeRedeemedFailure,
                new AuthorizationCodeDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                },
                error);

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseSuccessAuthorizationCodeRedeemedEventAsync(this IEventService events, Client client, string handle)
        {
            var evt = new Event<AuthorizationCodeDetails>(
                EventConstants.Categories.TokenService,
                "Authorization code redeem success",
                EventTypes.Success,
                EventConstants.Ids.AuthorizationCodeRedeemedSuccess,
                new AuthorizationCodeDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailedRefreshTokenRefreshEventAsync(this IEventService events, Client client, string handle, string error)
        {
            var evt = new Event<RefreshTokenDetails>(
                EventConstants.Categories.TokenService,
                "Refresh token refresh failure",
                EventTypes.Failure,
                EventConstants.Ids.RefreshTokenRefreshedFailure,
                new RefreshTokenDetails
                {
                    HandleId = handle,
                    ClientId = client.ClientId
                },
                error);

            await events.RaiseAsync(evt);
        }

        public static Task RaiseSuccessRefreshTokenRefreshEventAsync(this IEventService events, Client client, string handle)
        {
            return Task.FromResult(0);
        }

        public static async Task RaiseNoCertificateConfiguredEventAsync(this IEventService events)
        {
            var evt = new Event<object>(
                EventConstants.Categories.Information,
                "No signing certificate configured",
                EventTypes.Information,
                EventConstants.Ids.NoSigningCertificateConfigured);

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseCertificatePrivateKeyNotAccessibleEventAsync(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.InternalError,
                "Signing certificate has no private key, or key is not accessible",
                EventTypes.Error,
                EventConstants.Ids.SigningCertificatePrivateKeyNotAccessible,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                },
                "Make sure the account running your application has access to the private key");

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseCertificateKeyLengthTooShortEventAsync(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.InternalError,
                "Signing certificate key length is less than 2048 bits.",
                EventTypes.Error,
                EventConstants.Ids.SigningCertificatePrivateKeyNotAccessible,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseCertificateExpiringSoonEventAsync(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.Information,
                "The signing certificate will expire in the next 30 days",
                EventTypes.Information,
                EventConstants.Ids.SigningCertificateExpiringSoon,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseCertificateValidatedEventAsync(this IEventService events, X509Certificate2 cert)
        {
            var evt = new Event<SigningCertificateDetail>(
                EventConstants.Categories.Information,
                "Signing certificate validation success",
                EventTypes.Information,
                EventConstants.Ids.SigningCertificateValidated,
                new SigningCertificateDetail
                {
                    SigningCertificateName = cert.SubjectName.Name,
                    SigningCertificateExpiration = cert.NotAfter
                });

            await events.RaiseAsync(evt);
        }

        private static async Task RaiseEventAsync<T>(this IEventService events, Event<T> evt)
        {
            if (events == null) throw new ArgumentNullException("events");

            await events.RaiseAsync(evt);
        }
    }
}