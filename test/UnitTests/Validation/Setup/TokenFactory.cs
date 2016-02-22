// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Core;
using IdentityServer4.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Tests.Validation
{
    static class TokenFactory
    {
        public static Token CreateAccessToken(Client client, string subjectId, int lifetime, params string[] scopes)
        {
            var claims = new List<Claim> 
            {
                new Claim("client_id", client.ClientId),
                new Claim("sub", subjectId)
            };

            scopes.ToList().ForEach(s => claims.Add(new Claim("scope", s)));

            var token = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                Audience = "https://idsrv3.com/resources",
                Issuer = "https://idsrv3.com",
                Lifetime = lifetime,
                Claims = claims,
                Client = client
            };

            return token;
        }

        public static Token CreateAccessTokenLong(Client client, string subjectId, int lifetime, int count, params string[] scopes)
        {
            var claims = new List<Claim>
            {
                new Claim("client_id", client.ClientId),
                new Claim("sub", subjectId)
            };

            for (int i = 0; i < count; i++)
            {
                claims.Add(new Claim("junk", "x".Repeat(100)));
            }

            scopes.ToList().ForEach(s => claims.Add(new Claim("scope", s)));

            var token = new Token(OidcConstants.TokenTypes.AccessToken)
            {
                Audience = "https://idsrv3.com/resources",
                Issuer = "https://idsrv3.com",
                Lifetime = lifetime,
                Claims = claims,
                Client = client
            };

            return token;
        }

        public static Token CreateIdentityToken(string clientId, string subjectId)
        {
            var clients = Factory.CreateClientStore();

            var claims = new List<Claim> 
            {
                new Claim("sub", subjectId)
            };

            var token = new Token(OidcConstants.TokenTypes.IdentityToken)
            {
                Audience = clientId,
                Client = clients.FindClientByIdAsync(clientId).Result,
                Issuer = "https://idsrv3.com",
                Lifetime = 600,
                Claims = claims
            };

            return token;
        }

        public static Token CreateIdentityTokenLong(string clientId, string subjectId, int count)
        {
            var clients = Factory.CreateClientStore();

            var claims = new List<Claim>
            {
                new Claim("sub", subjectId)
            };

            for (int i = 0; i < count; i++)
            {
                claims.Add(new Claim("junk", "x".Repeat(100)));
            }

            var token = new Token(OidcConstants.TokenTypes.IdentityToken)
            {
                Audience = clientId,
                Client = clients.FindClientByIdAsync(clientId).Result,
                Issuer = "https://idsrv3.com",
                Lifetime = 600,
                Claims = claims
            };

            return token;
        }
    }
}