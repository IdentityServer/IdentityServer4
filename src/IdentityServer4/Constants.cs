// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Models;
using System;
using System.Collections.Generic;

#pragma warning disable 1591

namespace IdentityServer4.Core
{
    public static class Constants
    {
        public const string IdentityServerName = "IdentityServer4";
        
        public const string PrimaryAuthenticationType       = "idsrv";
        public const string ExternalAuthenticationType      = "idsrv.external";
        public const string PartialSignInAuthenticationType = "idsrv.partial";
        public const string ExternalAuthenticationMethod = "external";
        
        internal static readonly string[] IdentityServerAuthenticationTypes = new string[]
        {
            PrimaryAuthenticationType,
            ExternalAuthenticationType,
            PartialSignInAuthenticationType
        };
        
        public const string BuiltInIdentityProvider         = "idsrv";

        public const string AccessTokenAudience             = "{0}resources";

        public static readonly TimeSpan DefaultCookieTimeSpan = TimeSpan.FromHours(10);
        public static readonly TimeSpan ExternalCookieTimeSpan = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan DefaultRememberMeDuration = TimeSpan.FromDays(30);

        public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

        // the limit after which old messages are purged
        public const int SignInMessageThreshold = 5;

        public const string DefaultHashAlgorithm = "SHA256";

        public const string ScopeDisplayNameSuffix = "_DisplayName";
        public const string ScopeDescriptionSuffix = "_Description";
        
        public static readonly List<string> SupportedResponseTypes = new List<string> 
                            { 
                                OidcConstants.ResponseTypes.Code,
                                OidcConstants.ResponseTypes.Token,
                                OidcConstants.ResponseTypes.IdToken,
                                OidcConstants.ResponseTypes.IdTokenToken,
                                OidcConstants.ResponseTypes.CodeIdToken,
                                OidcConstants.ResponseTypes.CodeToken,
                                OidcConstants.ResponseTypes.CodeIdTokenToken
                            };

        public static readonly Dictionary<string, Flows> ResponseTypeToFlowMapping = new Dictionary<string, Flows>
                            {
                                { OidcConstants.ResponseTypes.Code, Flows.AuthorizationCode },
                                { OidcConstants.ResponseTypes.Token, Flows.Implicit },
                                { OidcConstants.ResponseTypes.IdToken, Flows.Implicit },
                                { OidcConstants.ResponseTypes.IdTokenToken, Flows.Implicit },
                                { OidcConstants.ResponseTypes.CodeIdToken, Flows.Hybrid },
                                { OidcConstants.ResponseTypes.CodeToken, Flows.Hybrid },
                                { OidcConstants.ResponseTypes.CodeIdTokenToken, Flows.Hybrid }
                            };

        public static readonly List<Flows> AllowedFlowsForAuthorizeEndpoint = new List<Flows>
                            {
                                Flows.AuthorizationCode,
                                Flows.Implicit,
                                Flows.Hybrid
                            };

        public enum ScopeRequirement
        {
            None, 
            ResourceOnly, 
            IdentityOnly,
            Identity
        }

        public static readonly Dictionary<string, ScopeRequirement> ResponseTypeToScopeRequirement = new Dictionary<string, ScopeRequirement>
                            {
                                { OidcConstants.ResponseTypes.Code, ScopeRequirement.None },
                                { OidcConstants.ResponseTypes.Token, ScopeRequirement.ResourceOnly },
                                { OidcConstants.ResponseTypes.IdToken, ScopeRequirement.IdentityOnly },
                                { OidcConstants.ResponseTypes.IdTokenToken, ScopeRequirement.Identity },
                                { OidcConstants.ResponseTypes.CodeIdToken, ScopeRequirement.Identity },
                                { OidcConstants.ResponseTypes.CodeToken, ScopeRequirement.Identity },
                                { OidcConstants.ResponseTypes.CodeIdTokenToken, ScopeRequirement.Identity }
                            };
                            
        public static readonly List<string> SupportedGrantTypes = new List<string> 
                            { 
                                OidcConstants.GrantTypes.AuthorizationCode,
                                OidcConstants.GrantTypes.ClientCredentials,
                                OidcConstants.GrantTypes.Password,
                                OidcConstants.GrantTypes.RefreshToken,
                                OidcConstants.GrantTypes.Implicit
                            };

        public static readonly Dictionary<Flows, IEnumerable<string>> AllowedResponseModesForFlow = new Dictionary<Flows, IEnumerable<string>>
                            {
                                { Flows.AuthorizationCode, new[] { OidcConstants.ResponseModes.Query, OidcConstants.ResponseModes.FormPost } },
                                { Flows.Implicit, new[] { OidcConstants.ResponseModes.Fragment, OidcConstants.ResponseModes.FormPost }},
                                { Flows.Hybrid, new[] { OidcConstants.ResponseModes.Fragment, OidcConstants.ResponseModes.FormPost }}
                            };

        public static readonly List<string> SupportedResponseModes = new List<string>
                            {
                                OidcConstants.ResponseModes.FormPost,
                                OidcConstants.ResponseModes.Query,
                                OidcConstants.ResponseModes.Fragment,
                            };

        public static string[] SupportedSubjectTypes =
        {
            "pairwise", "public"
        };

        public static class SigningAlgorithms
        {
            public const string RSA_SHA_256 = "RS256";
        }

        public static readonly List<string> SupportedDisplayModes = new List<string>
                            {
                                OidcConstants.DisplayModes.Page,
                                OidcConstants.DisplayModes.Popup,
                                OidcConstants.DisplayModes.Touch,
                                OidcConstants.DisplayModes.Wap,
                            };

        public static readonly List<string> SupportedPromptModes = new List<string>
                            {
                                OidcConstants.PromptModes.None,
                                OidcConstants.PromptModes.Login,
                                OidcConstants.PromptModes.Consent,
                                OidcConstants.PromptModes.SelectAccount,
                            };

        public static class KnownAcrValues
        {
            public const string HomeRealm = "idp:";
            public const string Tenant = "tenant:";

            public static readonly string[] All = { HomeRealm, Tenant };
        }

        public static Dictionary<string, int> ProtectedResourceErrorStatusCodes = new Dictionary<string, int>
        {
            { OidcConstants.ProtectedResourceErrors.InvalidToken,      401 },
            { OidcConstants.ProtectedResourceErrors.ExpiredToken,      401 },
            { OidcConstants.ProtectedResourceErrors.InvalidRequest,    400 },
            { OidcConstants.ProtectedResourceErrors.InsufficientScope, 403 },
        };
        
        public static readonly Dictionary<string, IEnumerable<string>> ScopeToClaimsMapping = new Dictionary<string, IEnumerable<string>>
        {
            { StandardScopes.Profile, new[]
                            { 
                                JwtClaimTypes.Name,
                                JwtClaimTypes.FamilyName,
                                JwtClaimTypes.GivenName,
                                JwtClaimTypes.MiddleName,
                                JwtClaimTypes.NickName,
                                JwtClaimTypes.PreferredUserName,
                                JwtClaimTypes.Profile,
                                JwtClaimTypes.Picture,
                                JwtClaimTypes.WebSite,
                                JwtClaimTypes.Gender,
                                JwtClaimTypes.BirthDate,
                                JwtClaimTypes.ZoneInfo,
                                JwtClaimTypes.Locale,
                                JwtClaimTypes.UpdatedAt 
                            }},
            { StandardScopes.Email, new[]
                            { 
                                JwtClaimTypes.Email,
                                JwtClaimTypes.EmailVerified 
                            }},
            { StandardScopes.Address, new[]
                            {
                                JwtClaimTypes.Address
                            }},
            { StandardScopes.Phone, new[]
                            {
                                JwtClaimTypes.PhoneNumber,
                                JwtClaimTypes.PhoneNumberVerified
                            }},
            { StandardScopes.OpenId, new[]
                            {
                                JwtClaimTypes.Subject
                            }},
        };

        public static class StandardScopes
        {
            /// <summary>REQUIRED. Informs the Authorization Server that the Client is making an OpenID Connect request. If the <c>openid</c> scope value is not present, the behavior is entirely unspecified.</summary>
            public const string OpenId        = "openid";
            /// <summary>OPTIONAL. This scope value requests access to the End-User's default profile Claims, which are: <c>name</c>, <c>family_name</c>, <c>given_name</c>, <c>middle_name</c>, <c>nickname</c>, <c>preferred_username</c>, <c>profile</c>, <c>picture</c>, <c>website</c>, <c>gender</c>, <c>birthdate</c>, <c>zoneinfo</c>, <c>locale</c>, and <c>updated_at</c>.</summary>
            public const string Profile       = "profile";
            /// <summary>OPTIONAL. This scope value requests access to the <c>email</c> and <c>email_verified</c> Claims.</summary>
            public const string Email         = "email";
            /// <summary>OPTIONAL. This scope value requests access to the <c>address</c> Claim.</summary>
            public const string Address       = "address";
            /// <summary>OPTIONAL. This scope value requests access to the <c>phone_number</c> and <c>phone_number_verified</c> Claims.</summary>
            public const string Phone         = "phone";
            /// <summary>This scope value MUST NOT be used with the OpenID Connect Implicit Client Implementer's Guide 1.0. See the OpenID Connect Basic Client Implementer's Guide 1.0 (http://openid.net/specs/openid-connect-implicit-1_0.html#OpenID.Basic) for its usage in that subset of OpenID Connect.</summary>
            public const string OfflineAccess = "offline_access";

            // not part of spec
            public const string AllClaims     = "all_claims";
            public const string Roles         = "roles";
        }

        public static class ClaimTypes
        {
            // claims for authentication controller partial logins
            public const string AuthorizationReturnUrl = "authorization_return_url";
            public const string PartialLoginRestartUrl = "partial_login_restart_url";
            public const string PartialLoginReturnUrl = "partial_login_return_url";

            // internal claim types
            // claim type to identify external user from external provider
            public const string ExternalProviderUserId = "external_provider_user_id";
            public const string PartialLoginResumeId = PartialLoginResumeClaimPrefix + "{0}";
        }

        public const string PartialLoginResumeClaimPrefix = "partial_login_resume_id:";

        public static readonly string[] ClaimsProviderFilerClaimTypes = new string[]
        {
            JwtClaimTypes.Audience,
            JwtClaimTypes.Issuer,
            JwtClaimTypes.NotBefore,
            JwtClaimTypes.Expiration,
            JwtClaimTypes.UpdatedAt,
            JwtClaimTypes.IssuedAt,
            JwtClaimTypes.AuthenticationMethod,
            JwtClaimTypes.AuthenticationTime,
            JwtClaimTypes.AuthorizedParty,
            JwtClaimTypes.AccessTokenHash,
            JwtClaimTypes.AuthorizationCodeHash,
            JwtClaimTypes.Nonce,
            JwtClaimTypes.IdentityProvider,
            JwtClaimTypes.SessionId
        };

        public static readonly string[] OidcProtocolClaimTypes = new string[]
        {
            JwtClaimTypes.Subject,
            JwtClaimTypes.AuthenticationMethod,
            JwtClaimTypes.IdentityProvider,
            JwtClaimTypes.AuthenticationTime,
            JwtClaimTypes.Audience,
            JwtClaimTypes.Issuer,
            JwtClaimTypes.NotBefore,
            JwtClaimTypes.Expiration,
            JwtClaimTypes.UpdatedAt,
            JwtClaimTypes.IssuedAt,
            JwtClaimTypes.AuthenticationContextClassReference,
            JwtClaimTypes.AuthorizedParty,
            JwtClaimTypes.AccessTokenHash,
            JwtClaimTypes.AuthorizationCodeHash,
            JwtClaimTypes.Nonce,
            JwtClaimTypes.JwtId,
            JwtClaimTypes.Scope,
            JwtClaimTypes.SessionId
        };

        public static readonly string[] AuthenticateResultClaimTypes = new string[]
        {
            JwtClaimTypes.Subject,
            JwtClaimTypes.Name,
            JwtClaimTypes.AuthenticationMethod,
            JwtClaimTypes.IdentityProvider,
            JwtClaimTypes.AuthenticationTime,
        };

        public static class ParsedSecretTypes
        {
            public const string SharedSecret = "SharedSecret";
            public const string X509Certificate = "X509Certificate";
        }

        public static class SecretTypes
        {
            public const string SharedSecret              = "SharedSecret";
            public const string X509CertificateThumbprint = "X509Thumbprint";
            public const string X509CertificateName       = "X509Name";
        }

        public static class RouteNames
        {
            public const string Welcome = "idsrv.welcome";
            public const string Login = "idsrv.authentication.login";
            public const string LoginExternal = "idsrv.authentication.loginexternal";
            public const string LoginExternalCallback = "idsrv.authentication.loginexternalcallback";
            public const string LogoutPrompt = "idsrv.authentication.logoutprompt";
            public const string Logout = "idsrv.authentication.logout";
            public const string ResumeLoginFromRedirect = "idsrv.authentication.resume";
            public const string CspReport = "idsrv.csp.report";
            public const string ClientPermissions = "idsrv.permissions";
            
            public static class Oidc
            {
                public const string AccessTokenValidation = "idsrv.oidc.accesstokenvalidation";
                public const string Authorize = "idsrv.oidc.authorize";
                public const string Consent = "idsrv.oidc.consent";
                public const string SwitchUser = "idsrv.oidc.switch";
                public const string CheckSession = "idsrv.oidc.checksession";
                public const string DiscoveryConfiguration = "idsrv.oidc.discoveryconfiguration";
                public const string DiscoveryWebKeys = "idsrv.oidc.discoverywebkeys";
                public const string EndSession = "idsrv.oidc.endsession";
                public const string EndSessionCallback = "idsrv.oidc.endsessioncallback";
                public const string IdentityTokenValidation = "idsrv.oidc.identitytokenvalidation";
                public const string Token = "idsrv.oidc.token";
                public const string Revocation = "idsrv.oidc.revocation";
                public const string UserInfo = "idsrv.oidc.userinfo";
                public const string Introspection = "idsrv.oidc.introspection";
            }
        }

        public static class RoutePaths
        {
            // TODO: new
            public const string Error = "ui/error";
            public const string Login = "ui/login";
            public const string Consent = "ui/consent";
            public const string Logout = "ui/logout";

            public const string CspReport = "csp/report";

            public static class Oidc
            {
                public const string Authorize = "connect/authorize";
                // new
                public const string AuthorizeAfterConsent = "connect/authorize/consent";
                public const string AuthorizeAfterLogin = "connect/authorize/login";

                //public const string Consent = "connect/consent";
                //public const string SwitchUser = "connect/switch";
                public const string DiscoveryConfiguration = ".well-known/openid-configuration";
                public const string DiscoveryWebKeys = DiscoveryConfiguration + "/jwks";
                public const string Token = "connect/token";
                public const string Revocation = "connect/revocation";
                public const string UserInfo = "connect/userinfo";
                //public const string AccessTokenValidation = "connect/accessTokenValidation";
                public const string Introspection = "connect/introspect";
                public const string IdentityTokenValidation = "connect/identityTokenValidation";
                public const string EndSession = "connect/endsession";
                //public const string EndSessionCallback = "connect/endsessioncallback";
                public const string CheckSession = "connect/checksession";
            }
            
            public static readonly string[] CorsPaths =
            {
                Oidc.DiscoveryConfiguration,
                Oidc.DiscoveryWebKeys,
                Oidc.Token,
                Oidc.UserInfo,
                Oidc.IdentityTokenValidation,
                Oidc.Revocation
            };
        }

        public static class OwinEnvironment
        {
            public const string IdentityServerBasePath = "idsrv:IdentityServerBasePath";
            public const string IdentityServerHost     = "idsrv:IdentityServerHost";
            public const string IdentityServerOrigin = "idsrv:IdentityServerOrigin";

            public const string RequestId = "idsrv:RequestId";
        }

        public static class Authentication
        {
            public const string SigninId                 = "signinid";
            public const string SignoutId                = "id";
            public const string KatanaAuthenticationType = "katanaAuthenticationType";
            public const string PartialLoginRememberMe = "idsvr:rememberme";
        }

        public static class LocalizationCategories
        {
            public const string Messages = "Messages";
            public const string Events   = "Events";
            public const string Scopes   = "Scopes";
        }

        public static class TokenTypeHints
        {
            public const string RefreshToken = "refresh_token";
            public const string AccessToken  = "access_token";
        }

        public static List<string> SupportedTokenTypeHints = new List<string>
        {
            TokenTypeHints.RefreshToken,
            TokenTypeHints.AccessToken
        };

        public static class RevocationErrors
        {
            public const string UnsupportedTokenType = "unsupported_token_type";
        }

        public static class ProfileDataCallers
        {
            public const string UserInfoEndpoint = "UserInfoEndpoint";
            public const string ClaimsProviderIdentityToken = "ClaimsProviderIdentityToken";
            public const string ClaimsProviderAccessToken = "ClaimsProviderAccessToken";
        }

        public static class ClaimValueTypes
        {
            public const string Json = "json";
        }
    }
}