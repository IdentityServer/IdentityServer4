// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Endpoints.Results;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
{
    public class DiscoveryEndpoint : IEndpoint
    {
        private readonly IdentityServerContext _context;
        private readonly CustomGrantValidator _customGrants;
        private readonly ISigningKeyService _keyService;
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly SecretParser _parsers;
        private readonly IScopeStore _scopes;

        public DiscoveryEndpoint(IdentityServerOptions options, IdentityServerContext context, IScopeStore scopes, ILogger<DiscoveryEndpoint> logger, ISigningKeyService keyService, CustomGrantValidator customGrants, SecretParser parsers)
        {
            _options = options;
            _scopes = scopes;
            _logger = logger;
            _keyService = keyService;
            _context = context;
            _customGrants = customGrants;
            _parsers = parsers;
        }

        public Task<IEndpointResult> ProcessAsync(IdentityServerContext context)
        {
            // validate HTTP
            if (context.HttpContext.Request.Method != "GET")
            {
                // todo
                // return bad request or 405 ?
            }

            if (context.HttpContext.Request.Path.Value.EndsWith("/jwks"))
            {
                return ExecuteJwksAsync(context.HttpContext);
            }
            else
            {
                return ExecuteDiscoDocAsync(context.HttpContext);
            }
        }

        private async Task<IEndpointResult> ExecuteDiscoDocAsync(HttpContext context)
        {
            _logger.LogVerbose("Start discovery request");

            var baseUrl = _context.GetIdentityServerBaseUrl().EnsureTrailingSlash();
            var allScopes = await _scopes.GetScopesAsync(publicOnly: true);
            var showScopes = new List<Scope>();

            var document = new DiscoveryDocument
            {
                issuer = _context.GetIssuerUri(),
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { Constants.SigningAlgorithms.RSA_SHA_256 }
            };

            // scopes
            if (_options.DiscoveryOptions.ShowIdentityScopes)
            {
                showScopes.AddRange(allScopes.Where(s => s.Type == ScopeType.Identity));
            }
            if (_options.DiscoveryOptions.ShowResourceScopes)
            {
                showScopes.AddRange(allScopes.Where(s => s.Type == ScopeType.Resource));
            }

            if (showScopes.Any())
            {
                document.scopes_supported = showScopes.Where(s => s.ShowInDiscoveryDocument).Select(s => s.Name).ToArray();
            }

            // claims
            if (_options.DiscoveryOptions.ShowClaims)
            {
                var claims = new List<string>();
                foreach (var s in allScopes)
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
                var standardGrantTypes = Constants.SupportedGrantTypes.AsEnumerable();
                if (this._options.AuthenticationOptions.EnableLocalLogin == false)
                {
                    standardGrantTypes = standardGrantTypes.Where(type => type != OidcConstants.GrantTypes.Password);
                }

                var showGrantTypes = new List<string>(standardGrantTypes);

                if (_options.DiscoveryOptions.ShowCustomGrantTypes)
                {
                    showGrantTypes.AddRange(_customGrants.GetAvailableGrantTypes());
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
                if (_options.Endpoints.EnableEndSessionEndpoint)
                {
                    document.http_logout_supported = true;
                }

                if (_options.Endpoints.EnableAuthorizeEndpoint)
                {
                    document.authorization_endpoint = baseUrl + Constants.RoutePaths.Oidc.Authorize;
                }

                if (_options.Endpoints.EnableTokenEndpoint)
                {
                    document.token_endpoint = baseUrl + Constants.RoutePaths.Oidc.Token;
                }

                if (_options.Endpoints.EnableUserInfoEndpoint)
                {
                    document.userinfo_endpoint = baseUrl + Constants.RoutePaths.Oidc.UserInfo;
                }

                if (_options.Endpoints.EnableEndSessionEndpoint)
                {
                    document.end_session_endpoint = baseUrl + Constants.RoutePaths.Oidc.EndSession;
                }

                if (_options.Endpoints.EnableCheckSessionEndpoint)
                {
                    document.check_session_iframe = baseUrl + Constants.RoutePaths.Oidc.CheckSession;
                }

                if (_options.Endpoints.EnableTokenRevocationEndpoint)
                {
                    document.revocation_endpoint = baseUrl + Constants.RoutePaths.Oidc.Revocation;
                }

                if (_options.Endpoints.EnableIntrospectionEndpoint)
                {
                    document.introspection_endpoint = baseUrl + Constants.RoutePaths.Oidc.Introspection;
                }
            }

            if (_options.DiscoveryOptions.ShowKeySet)
            {
                if (_options.SigningCertificate != null)
                {
                    document.jwks_uri = baseUrl + Constants.RoutePaths.Oidc.DiscoveryWebKeys;
                }
            }

            return new DiscoveryDocumentResult(document, _options.DiscoveryOptions.CustomEntries); 
        }

        private async Task<IEndpointResult> ExecuteJwksAsync(HttpContext context)
        {
            _logger.LogVerbose("Start key discovery request");

            if (_options.DiscoveryOptions.ShowKeySet == false)
            {
                _logger.LogInformation("Key discovery disabled. 404.");
                return new StatusCodeResult(404);
            }

            var webKeys = new List<JsonWebKey>();
            foreach (var pubKey in await _keyService.GetValidationKeysAsync())
            {
                if (pubKey != null)
                {
                    var cert64 = Convert.ToBase64String(pubKey.RawData);
                    var thumbprint = Base64Url.Encode(pubKey.GetCertHash());

                    // todo
                    //var key = pubKey.PublicKey.Key as RSACryptoServiceProvider;
                    //var parameters = key.ExportParameters(false);
                    //var exponent = Base64Url.Encode(parameters.Exponent);
                    //var modulus = Base64Url.Encode(parameters.Modulus);

                    var webKey = new JsonWebKey
                    {
                        kty = "RSA",
                        use = "sig",
                        kid = await _keyService.GetKidAsync(pubKey),
                        x5t = thumbprint,
                        //e = exponent,
                        //n = modulus,
                        x5c = new[] { cert64 }
                    };

                    webKeys.Add(webKey);
                }
            }

            return new JsonWebKeysResult(webKeys);
        }
    }
}