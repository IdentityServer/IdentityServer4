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
        private readonly IResourceStore _resourceStore;

        public DiscoveryEndpoint(IdentityServerOptions options, IResourceStore resourceStore, ILogger<DiscoveryEndpoint> logger, IKeyMaterialService keys, ExtensionGrantValidator extensionGrants, SecretParser parsers, IResourceOwnerPasswordValidator resourceOwnerValidator)
        {
            _options = options;
            _resourceStore = resourceStore;
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
                return ExecuteJwksAsync();
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

            var document = new DiscoveryDocument
            {
                issuer = context.GetIdentityServerIssuerUri(),
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { Constants.SigningAlgorithms.RSA_SHA_256 },
                code_challenge_methods_supported = new[] { OidcConstants.CodeChallengeMethods.Plain, OidcConstants.CodeChallengeMethods.Sha256 }
            };

            // check to see if we need to load resources
            if (_options.Discovery.ShowIdentityScopes ||
                _options.Discovery.ShowApiScopes ||
                _options.Discovery.ShowClaims)
            {
                var resources = await _resourceStore.GetAllEnabledResourcesAsync();
                var scopes = new List<string>();

                // scopes
                if (_options.Discovery.ShowIdentityScopes)
                {
                    scopes.AddRange(resources.IdentityResources.Where(x=>x.ShowInDiscoveryDocument).Select(x=>x.Name));
                }
                if (_options.Discovery.ShowApiScopes)
                {
                    var apiScopes = from api in resources.ApiResources
                                    from scope in api.Scopes
                                    where scope.ShowInDiscoveryDocument
                                    select scope.Name;
                    scopes.AddRange(apiScopes);
                    scopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
                }

                if (scopes.Any())
                {
                    document.scopes_supported = scopes.ToArray();
                }

                // claims
                if (_options.Discovery.ShowClaims)
                {
                    var claims = new List<string>();

                    claims.AddRange(resources.IdentityResources.SelectMany(x => x.UserClaims));
                    claims.AddRange(resources.ApiResources.SelectMany(x => x.UserClaims));

                    document.claims_supported = claims.Distinct().ToArray();
                }
            }

            // grant types
            if (_options.Discovery.ShowGrantTypes)
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

                if (_options.Discovery.ShowExtensionGrantTypes)
                {
                    showGrantTypes.AddRange(_extensionGrants.GetAvailableGrantTypes());
                }

                document.grant_types_supported = showGrantTypes.ToArray();
            }

            // response types
            if (_options.Discovery.ShowResponseTypes)
            {
                document.response_types_supported = Constants.SupportedResponseTypes.ToArray();
            }

            // response modes
            if (_options.Discovery.ShowResponseModes)
            {
                document.response_modes_supported = Constants.SupportedResponseModes.ToArray();
            }

            // token endpoint authentication methods
            if (_options.Discovery.ShowTokenEndpointAuthenticationMethods)
            {
                document.token_endpoint_auth_methods_supported = _parsers.GetAvailableAuthenticationMethods().ToArray();
            }

            // endpoints
            if (_options.Discovery.ShowEndpoints)
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

            if (_options.Discovery.ShowKeySet)
            {
                if ((await _keys.GetValidationKeysAsync()).Any())
                {
                    document.jwks_uri = baseUrl + Constants.ProtocolRoutePaths.DiscoveryWebKeys;
                }
            }

            return new DiscoveryDocumentResult(document, _options.Discovery.CustomEntries);
        }

        private async Task<IEndpointResult> ExecuteJwksAsync()
        {
            _logger.LogDebug("Start key discovery request");

            if (_options.Discovery.ShowKeySet == false)
            {
                _logger.LogInformation("Key discovery disabled. 404.");
                return new StatusCodeResult(404);
            }

            var webKeys = new List<Models.JsonWebKey>();
            foreach (var key in await _keys.GetValidationKeysAsync())
            {
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