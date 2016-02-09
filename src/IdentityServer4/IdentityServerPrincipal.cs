// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Core
{
    internal static class IdentityServerPrincipal
    {
        public static ClaimsPrincipal Create(
            string subject,
            string displayName,
            string authenticationMethod = OidcConstants.AuthenticationMethods.Password,
            string idp = Constants.BuiltInIdentityProvider,
            string authenticationType = Constants.PrimaryAuthenticationType,
            long authenticationTime = 0)
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(displayName)) throw new ArgumentNullException("displayName");
            if (String.IsNullOrWhiteSpace(authenticationMethod)) throw new ArgumentNullException("authenticationMethod");
            if (String.IsNullOrWhiteSpace(idp)) throw new ArgumentNullException("idp");
            if (String.IsNullOrWhiteSpace(authenticationType)) throw new ArgumentNullException("authenticationType");

            if (authenticationTime <= 0) authenticationTime = DateTimeOffset.UtcNow.ToEpochTime();

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, subject),
                new Claim(JwtClaimTypes.Name, displayName),
                new Claim(JwtClaimTypes.AuthenticationMethod, authenticationMethod),
                new Claim(JwtClaimTypes.IdentityProvider, idp),
                new Claim(JwtClaimTypes.AuthenticationTime, authenticationTime.ToString(), ClaimValueTypes.Integer)
            };

            var id = new ClaimsIdentity(claims, authenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        public static ClaimsPrincipal CreateFromPrincipal(ClaimsPrincipal principal, string authenticationType)
        {
            // we require the following claims
            var subject = principal.FindFirst(JwtClaimTypes.Subject);
            if (subject == null) throw new InvalidOperationException("sub claim is missing");

            var name = principal.FindFirst(JwtClaimTypes.Name);
            if (name == null) throw new InvalidOperationException("name claim is missing");

            var authenticationMethod = principal.FindFirst(JwtClaimTypes.AuthenticationMethod);
            if (authenticationMethod == null) throw new InvalidOperationException("amr claim is missing");

            var authenticationTime = principal.FindFirst(JwtClaimTypes.AuthenticationTime);
            if (authenticationTime == null) throw new InvalidOperationException("auth_time claim is missing");

            var idp = principal.FindFirst(JwtClaimTypes.IdentityProvider);
            if (idp == null) throw new InvalidOperationException("idp claim is missing");

            var id = new ClaimsIdentity(principal.Claims, authenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        public static ClaimsPrincipal FromSubjectId(string subjectId, IEnumerable<Claim> additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, subjectId)
            };

            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            return Principal.Create(Constants.PrimaryAuthenticationType,
                claims.Distinct(new ClaimComparer()).ToArray());
        }

        public static ClaimsPrincipal FromClaims(IEnumerable<Claim> claims, bool allowMissing = false)
        {
            var amr = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.AuthenticationMethod);
            var sub = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);
            var idp = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.IdentityProvider);
            var authTime = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.AuthenticationTime);

            var id = new ClaimsIdentity(Constants.BuiltInIdentityProvider);

            if (sub != null)
            {
                id.AddClaim(sub);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("sub claim is missing");
                }
            }

            if (amr != null)
            {
                id.AddClaim(amr);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("amr claim is missing");
                }
            }

            if (idp != null)
            {
                id.AddClaim(idp);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("idp claim is missing");
                }
            }

            if (authTime != null)
            {
                id.AddClaim(authTime);
            }
            else
            {
                if (allowMissing == false)
                {
                    throw new InvalidOperationException("auth_time claim is missing");
                }
            }

            return new ClaimsPrincipal(id);
        }
    }
}