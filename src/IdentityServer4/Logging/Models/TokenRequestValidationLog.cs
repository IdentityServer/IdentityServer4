// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Validation;
using System.Collections.Generic;

namespace IdentityServer4.Core.Logging
{
    internal class TokenRequestValidationLog
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string GrantType { get; set; }
        public string Scopes { get; set; }

        public string AuthorizationCode { get; set; }
        public string RefreshToken { get; set; }

        public string UserName { get; set; }
        public IEnumerable<string> AuthenticationContextReferenceClasses { get; set; }
        public string Tenant { get; set; }
        public string IdP { get; set; }

        public Dictionary<string, string> Raw { get; set; }

        public TokenRequestValidationLog(ValidatedTokenRequest request)
        {
            const string scrubValue = "******";
            Raw = request.Raw.ToDictionary();

            if (Raw.ContainsKey(OidcConstants.TokenRequest.Password))
            {
                Raw[OidcConstants.TokenRequest.Password] = scrubValue;
            }

            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;
            }

            if (request.Scopes != null)
            {
                Scopes = request.Scopes.ToSpaceSeparatedString();
            }

            if (request.SignInMessage != null)
            {
                IdP = request.SignInMessage.IdP;
                Tenant = request.SignInMessage.Tenant;
                AuthenticationContextReferenceClasses = request.SignInMessage.AcrValues;
            }

            GrantType = request.GrantType;
            AuthorizationCode = request.AuthorizationCodeHandle;
            RefreshToken = request.RefreshTokenHandle;
            UserName = request.UserName;
        }
    }
}