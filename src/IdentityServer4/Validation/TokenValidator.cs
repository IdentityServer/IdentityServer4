// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Logging;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

#if DOTNET5_4
using System.IdentityModel.Tokens.Jwt;
#endif

namespace IdentityServer4.Core.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TokenValidator : ITokenValidator
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IdentityServerContext _context;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly ICustomTokenValidator _customValidator;
        private readonly IClientStore _clients;
        private readonly ISigningKeyService _keyService;

        private readonly TokenValidationLog _log;
        
        public TokenValidator(IdentityServerOptions options, IdentityServerContext context, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator, ISigningKeyService keyService, ILogger<TokenValidator> logger)
        {
            _options = options;
            _context = context;
            _clients = clients;
            _tokenHandles = tokenHandles;
            _customValidator = customValidator;
            _keyService = keyService;
            _logger = logger;

            _log = new TokenValidationLog();
        }
        
        public virtual async Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
        {
            _logger.LogVerbose("Start identity token validation");

            if (token.Length > _options.InputLengthRestrictions.Jwt)
            {
                _logger.LogError("JWT too long");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            if (clientId.IsMissing())
            {
                clientId = GetClientIdFromJwt(token);

                if (clientId.IsMissing())
                {
                    _logger.LogError("No clientId supplied, can't find id in identity token.");
                    return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
                }
            }

            _log.ClientId = clientId;
            _log.ValidateLifetime = validateLifetime;

            var client = await _clients.FindClientByIdAsync(clientId);
            if (client == null)
            {
                LogError("Unknown or diabled client.");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            _log.ClientName = client.ClientName;

            var certs = await _keyService.GetValidationKeysAsync();
            var result = await ValidateJwtAsync(token, clientId, certs, validateLifetime);

            result.Client = client;

            if (result.IsError)
            {
                LogError("Error validating JWT");
                return result;
            }

            _log.Claims = result.Claims.ToClaimsDictionary();

            var customResult = await _customValidator.ValidateIdentityTokenAsync(result);

            if (customResult.IsError)
            {
                LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
                return customResult;
            }

            _log.Claims = customResult.Claims.ToClaimsDictionary();

            LogSuccess();
            return customResult;
        }

        public virtual async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            _logger.LogVerbose("Start access token validation");

            _log.ExpectedScope = expectedScope;
            _log.ValidateLifetime = true;

            TokenValidationResult result;

            if (token.Contains("."))
            {
                if (token.Length > _options.InputLengthRestrictions.Jwt)
                {
                    _logger.LogError("JWT too long");

                    return new TokenValidationResult
                    {
                        IsError = true,
                        Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                        ErrorDescription = "Token too long"
                    };
                }

                _log.AccessTokenType = AccessTokenType.Jwt.ToString();
                result = await ValidateJwtAsync(
                    token,
                    string.Format(Constants.AccessTokenAudience, _context.GetIssuerUri().EnsureTrailingSlash()),
                    await _keyService.GetValidationKeysAsync());
            }
            else
            {
                if (token.Length > _options.InputLengthRestrictions.TokenHandle)
                {
                    _logger.LogError("token handle too long");

                    return new TokenValidationResult
                    {
                        IsError = true,
                        Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                        ErrorDescription = "Token too long"
                    };
                }

                _log.AccessTokenType = AccessTokenType.Reference.ToString();
                result = await ValidateReferenceAccessTokenAsync(token);
            }

            _log.Claims = result.Claims.ToClaimsDictionary();

            if (result.IsError)
            {
                return result;
            }

            if (expectedScope.IsPresent())
            {
                var scope = result.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Scope && c.Value == expectedScope);
                if (scope == null)
                {
                    LogError(string.Format("Checking for expected scope {0} failed", expectedScope));
                    return Invalid(OidcConstants.ProtectedResourceErrors.InsufficientScope);
                }
            }

            var customResult = await _customValidator.ValidateAccessTokenAsync(result);

            if (customResult.IsError)
            {
                LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
                return customResult;
            }

            // add claims again after custom validation
            _log.Claims = customResult.Claims.ToClaimsDictionary();

            LogSuccess();
            return customResult;
        }

        private async Task<TokenValidationResult> ValidateJwtAsync(string jwt, string audience, IEnumerable<X509Certificate2> signingCertificates, bool validateLifetime = true)
        {
            var handler = new JwtSecurityTokenHandler();

            // todo
#if NET451
            JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();
#elif DOTNET5_4
            handler.InboundClaimTypeMap.Clear();
#endif


            //{
            //    Configuration =
            //        new SecurityTokenHandlerConfiguration
            //        {
            //            CertificateValidationMode = X509CertificateValidationMode.None,
            //            CertificateValidator = X509CertificateValidator.None
            //        }
            //};

            var keys = (from c in signingCertificates select new X509SecurityKey(c)).ToList();

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _context.GetIssuerUri(),
                IssuerSigningKeys = keys,
                ValidateLifetime = validateLifetime,
                ValidAudience = audience
            };

            try
            {
                SecurityToken jwtToken;
                var id = handler.ValidateToken(jwt, parameters, out jwtToken);

                // if access token contains an ID, log it
                var jwtId = id.FindFirst(JwtClaimTypes.JwtId);
                if (jwtId != null)
                {
                    _log.JwtId = jwtId.Value;
                }

                // load the client that belongs to the client_id claim
                Client client = null;
                var clientId = id.FindFirst(JwtClaimTypes.ClientId);
                if (clientId != null)
                {
                    client = await _clients.FindClientByIdAsync(clientId.Value);
                    if (client == null)
                    {
                        throw new InvalidOperationException("Client does not exist anymore.");
                    }
                }

                return new TokenValidationResult
                {
                    IsError = false,

                    Claims = id.Claims,
                    Client = client,
                    Jwt = jwt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("JWT token validation error", ex);
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }
        }

        private async Task<TokenValidationResult> ValidateReferenceAccessTokenAsync(string tokenHandle)
        {
            _log.TokenHandle = tokenHandle;
            var token = await _tokenHandles.GetAsync(tokenHandle);

            if (token == null)
            {
                LogError("Token handle not found");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            if (token.Type != OidcConstants.TokenTypes.AccessToken)
            {
                LogError("Token handle does not resolve to an access token - but instead to: " + token.Type);

                await _tokenHandles.RemoveAsync(tokenHandle);
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            if (DateTimeOffsetHelper.UtcNow >= token.CreationTime.AddSeconds(token.Lifetime))
            {
                LogError("Token expired.");

                await _tokenHandles.RemoveAsync(tokenHandle);
                return Invalid(OidcConstants.ProtectedResourceErrors.ExpiredToken);
            }

            return new TokenValidationResult
            {
                IsError = false,

                Client = token.Client,
                Claims = ReferenceTokenToClaims(token),
                ReferenceToken = token,
                ReferenceTokenId = tokenHandle
            };
        }

        private IEnumerable<Claim> ReferenceTokenToClaims(Token token)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Audience, token.Audience),
                new Claim(JwtClaimTypes.Issuer, token.Issuer),
                new Claim(JwtClaimTypes.NotBefore, token.CreationTime.ToEpochTime().ToString()),
                new Claim(JwtClaimTypes.Expiration, token.CreationTime.AddSeconds(token.Lifetime).ToEpochTime().ToString())
            };

            claims.AddRange(token.Claims);

            return claims;
        }

        private string GetClientIdFromJwt(string token)
        {
            try
            {
                var jwt = new JwtSecurityToken(token);
                var clientId = jwt.Audiences.FirstOrDefault();

                return clientId;
            }
            catch (Exception ex)
            {
                _logger.LogError("Malformed JWT token", ex);
                return null;
            }
        }

        private TokenValidationResult Invalid(string error)
        {
            return new TokenValidationResult
            {
                IsError = true,
                Error = error
            };
        }

        private void LogError(string message)
        {
            var json = LogSerializer.Serialize(_log);
            _logger.LogError(message +"\n{logMessage}",json);
        }

        private void LogSuccess()
        {
            var json = LogSerializer.Serialize(_log);
            _logger.LogInformation("Token validation success\n{logMessage}", json);
        }
    }
}