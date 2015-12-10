using IdentityModel;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Services;
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
        private readonly ISigningKeyService _keyService;
        private readonly ILogger _logger;
        private IdentityServerOptions _options;
        private readonly IScopeStore _scopes;

        public DiscoveryEndpoint(IdentityServerOptions options, IScopeStore scopes, ILogger<DiscoveryEndpoint> logger, ISigningKeyService keyService)
        {
            _options = options;
            _scopes = scopes;
            _logger = logger;
            _keyService = keyService;
        }

        public Task<IResult> ProcessAsync(HttpContext context)
        {
            // validate HTTP
            if (context.Request.Method != "GET")
            {
                // todo
                // return bad request or 405 ?
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

        private async Task<IResult> ExecuteDiscoDocAsync(HttpContext context)
        {
            _logger.LogVerbose("Start discovery request");

            var baseUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash();
            var scopes = await _scopes.GetScopesAsync(publicOnly: true);

            var claims = new List<string>();
            foreach (var s in scopes)
            {
                claims.AddRange(from c in s.Claims
                                where s.Type == ScopeType.Identity
                                select c.Name);
            }

            var supportedGrantTypes = Constants.SupportedGrantTypes.AsEnumerable();
            if (_options.AuthenticationOptions.EnableLocalLogin == false)
            {
                supportedGrantTypes = supportedGrantTypes.Where(type => type != Constants.GrantTypes.Password);
            }

            var document = new DiscoveryDocument
            {
                issuer = context.GetIdentityServerIssuerUri(),
                scopes_supported = scopes.Where(s => s.ShowInDiscoveryDocument).Select(s => s.Name).ToArray(),
                claims_supported = claims.Distinct().ToArray(),
                response_types_supported = Constants.SupportedResponseTypes.ToArray(),
                response_modes_supported = Constants.SupportedResponseModes.ToArray(),
                grant_types_supported = supportedGrantTypes.ToArray(),
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { Constants.SigningAlgorithms.RSA_SHA_256 },
                token_endpoint_auth_methods_supported = new[] { Constants.TokenEndpointAuthenticationMethods.PostBody, Constants.TokenEndpointAuthenticationMethods.BasicAuthentication },
            };

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

            if (_options.SigningCertificate != null)
            {
                document.jwks_uri = baseUrl + Constants.RoutePaths.Oidc.DiscoveryWebKeys;
            }

            return new DiscoveryDocumentResult(document);
        }

        private async Task<IResult> ExecuteJwksAsync(HttpContext context)
        {
            _logger.LogVerbose("Start key discovery request");

            var webKeys = new List<JsonWebKey>();
            foreach (var pubKey in await _keyService.GetPublicKeysAsync())
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