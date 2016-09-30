﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints
{
    public class DiscoveryEndpoint : IEndpoint
    {
        private readonly IdentityServerOptions _options;
        private readonly ExtensionGrantValidator _extensionGrants;
        private readonly IKeyMaterialService _keys;
        private readonly ILogger _logger;
        private readonly SecretParser _parsers;
        private readonly IResourceOwnerPasswordValidator _resourceOwnerValidator;
        private readonly IScopeStore _scopes;

        public DiscoveryEndpoint(IdentityServerOptions options, IScopeStore scopes, ILogger<DiscoveryEndpoint> logger, IKeyMaterialService keys, ExtensionGrantValidator extensionGrants, SecretParser parsers, IResourceOwnerPasswordValidator resourceOwnerValidator)
        {
            _options = options;
            _scopes = scopes;
            _logger = logger;
            _extensionGrants = extensionGrants;
            _parsers = parsers;
            _keys = keys;
            _resourceOwnerValidator = resourceOwnerValidator;
        }

        public Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing discovery request.");

            // validate HTTP
            if (context.Request.Method != "GET")
            {
                _logger.LogWarning("Discovery endpoint only supports GET requests");
                return Task.FromResult<IEndpointResult>(new StatusCodeResult(HttpStatusCode.MethodNotAllowed));
            }

            if (context.Request.Path.Value.EndsWith("/jwks"))
            {
                return ExecuteJwksAsync(context);
            }
            else
            {
                return ExecuteDiscoDocAsync(context);
            }
        }

        private async Task<IEndpointResult> ExecuteDiscoDocAsync(HttpContext context)
        {
            _logger.LogDebug("Start discovery request");

            if (!_options.Endpoints.EnableDiscoveryEndpoint)
            {
                _logger.LogInformation("Discovery endpoint disabled. 404.");
                return new StatusCodeResult(404);
            }

            var baseUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash();
            var allScopes = await _scopes.GetEnabledScopesAsync(publicOnly: true);
            var showScopes = new List<Scope>();

            var document = new DiscoveryDocument
            {
                issuer = context.GetIssuerUri(),
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { Constants.SigningAlgorithms.RSA_SHA_256 },
                code_challenge_methods_supported = new[] { OidcConstants.CodeChallengeMethods.Plain, OidcConstants.CodeChallengeMethods.Sha256 }
            };

            // scopes
            var theScopes = allScopes as Scope[] ?? allScopes.ToArray();
            if (_options.DiscoveryOptions.ShowIdentityScopes)
            {
                showScopes.AddRange(theScopes.Where(s => s.Type == ScopeType.Identity));
            }
            if (_options.DiscoveryOptions.ShowResourceScopes)
            {
                showScopes.AddRange(theScopes.Where(s => s.Type == ScopeType.Resource));
            }

            if (showScopes.Any())
            {
                document.scopes_supported = showScopes.Where(s => s.ShowInDiscoveryDocument).Select(s => s.Name).ToArray();
            }

            // claims
            if (_options.DiscoveryOptions.ShowClaims)
            {
                var claims = new List<string>();
                foreach (var s in theScopes)
                {
                    claims.AddRange(from c in s.Claims
                                    where s.Type == ScopeType.Identity
                                    select c.Name);
                }

                document.claims_supported = claims.Distinct().ToArray();
            }

            // grant types
            if (_options.DiscoveryOptions.ShowGrantTypes)
            {
                var standardGrantTypes = new List<string>
                {
                    OidcConstants.GrantTypes.AuthorizationCode,
                    OidcConstants.GrantTypes.ClientCredentials,
                    OidcConstants.GrantTypes.RefreshToken,
                    OidcConstants.GrantTypes.Implicit
                };

                if (!(_resourceOwnerValidator is NotSupportedResouceOwnerPasswordValidator))
                {
                    standardGrantTypes.Add(OidcConstants.GrantTypes.Password);
                }
                
                var showGrantTypes = new List<string>(standardGrantTypes);

                if (_options.DiscoveryOptions.ShowExtensionGrantTypes)
                {
                    showGrantTypes.AddRange(_extensionGrants.GetAvailableGrantTypes());
                }

                document.grant_types_supported = showGrantTypes.ToArray();
            }

            // response types
            if (_options.DiscoveryOptions.ShowResponseTypes)
            {
                document.response_types_supported = Constants.SupportedResponseTypes.ToArray();
            }

            // response modes
            if (_options.DiscoveryOptions.ShowResponseModes)
            {
                document.response_modes_supported = Constants.SupportedResponseModes.ToArray();
            }

            // token endpoint authentication methods
            if (_options.DiscoveryOptions.ShowTokenEndpointAuthenticationMethods)
            {
                document.token_endpoint_auth_methods_supported = _parsers.GetAvailableAuthenticationMethods().ToArray();
            }

            // endpoints
            if (_options.DiscoveryOptions.ShowEndpoints)
            {
                if (_options.Endpoints.EnableAuthorizeEndpoint)
                {
                    document.authorization_endpoint = baseUrl + Constants.ProtocolRoutePaths.Authorize;
                }

                if (_options.Endpoints.EnableTokenEndpoint)
                {
                    document.token_endpoint = baseUrl + Constants.ProtocolRoutePaths.Token;
                }

                if (_options.Endpoints.EnableUserInfoEndpoint)
                {
                    document.userinfo_endpoint = baseUrl + Constants.ProtocolRoutePaths.UserInfo;
                }

                if (_options.Endpoints.EnableEndSessionEndpoint)
                {
                    document.frontchannel_logout_session_supported = true;
                    document.frontchannel_logout_supported = true;
                    document.end_session_endpoint = baseUrl + Constants.ProtocolRoutePaths.EndSession;
                }

                if (_options.Endpoints.EnableCheckSessionEndpoint)
                {
                    document.check_session_iframe = baseUrl + Constants.ProtocolRoutePaths.CheckSession;
                }

                if (_options.Endpoints.EnableTokenRevocationEndpoint)
                {
                    document.revocation_endpoint = baseUrl + Constants.ProtocolRoutePaths.Revocation;
                }

                if (_options.Endpoints.EnableIntrospectionEndpoint)
                {
                    document.introspection_endpoint = baseUrl + Constants.ProtocolRoutePaths.Introspection;
                }
            }

            if (_options.DiscoveryOptions.ShowKeySet)
            {
                if ((await _keys.GetValidationKeysAsync()).Any())
                {
                    document.jwks_uri = baseUrl + Constants.ProtocolRoutePaths.DiscoveryWebKeys;
                }
            }

            return new DiscoveryDocumentResult(document, _options.DiscoveryOptions.CustomEntries);
        }

        private async Task<IEndpointResult> ExecuteJwksAsync(HttpContext context)
        {
            _logger.LogDebug("Start key discovery request");

            if (_options.DiscoveryOptions.ShowKeySet == false)
            {
                _logger.LogInformation("Key discovery disabled. 404.");
                return new StatusCodeResult(404);
            }

            var webKeys = new List<Models.JsonWebKey>();
            foreach (var key in await _keys.GetValidationKeysAsync())
            {
                // todo
                //if (!(key is AsymmetricSecurityKey) &&
                //     !key.IsSupportedAlgorithm(SecurityAlgorithms.RsaSha256Signature))
                //{
                //    var error = "signing key is not asymmetric and does not support RS256";
                //    _logger.LogError(error);
                //    throw new InvalidOperationException(error);
                //}
              
                var x509Key = key as X509SecurityKey;
                if (x509Key != null)
                {
                    var cert64 = Convert.ToBase64String(x509Key.Certificate.RawData);
                    var thumbprint = Base64Url.Encode(x509Key.Certificate.GetCertHash());

                    var pubKey = x509Key.PublicKey as RSA;
                    var parameters = pubKey.ExportParameters(false);
                    var exponent = Base64Url.Encode(parameters.Exponent);
                    var modulus = Base64Url.Encode(parameters.Modulus);

                    var webKey = new Models.JsonWebKey
                    {
                        kty = "RSA",
                        use = "sig",
                        kid = x509Key.KeyId,
                        x5t = thumbprint,
                        e = exponent,
                        n = modulus,
                        x5c = new[] { cert64 }
                    };

                    webKeys.Add(webKey);
                    continue;
                }

                var rsaKey = key as RsaSecurityKey;
                if (rsaKey != null)
                {
                    var parameters = rsaKey.Rsa?.ExportParameters(false) ?? rsaKey.Parameters;
                    var exponent = Base64Url.Encode(parameters.Exponent);
                    var modulus = Base64Url.Encode(parameters.Modulus);

                    var webKey = new Models.JsonWebKey
                    {
                        kty = "RSA",
                        use = "sig",
                        kid = rsaKey.KeyId,
                        e = exponent,
                        n = modulus,
                    };

                    webKeys.Add(webKey);
                }
            }

            return new JsonWebKeysResult(webKeys);
        }
    }
}