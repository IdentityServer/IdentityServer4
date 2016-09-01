// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4
{
    internal static class IdentityServerPrincipal
    {
        public static ClaimsPrincipal Create(
            string subject,
            string name,
            params Claim[] claims)
        {
            return Create(subject, name, Constants.LocalIdentityProvider, new[] { OidcConstants.AuthenticationMethods.Password }, claims);
        }

        public static ClaimsPrincipal Create(
            string subject,
            string name,
            string idp,
            params Claim[] claims)
        {
            return Create(subject, name, idp, new[] { Constants.ExternalAuthenticationMethod }, claims);
        }

        public static ClaimsPrincipal Create(
            string subject,
            string name,
            IEnumerable<string> amr,
            params Claim[] claims)
        {
            return Create(subject, name, Constants.LocalIdentityProvider, amr, claims);
        }

        public static ClaimsPrincipal Create(
            string subject,
            string name,
            string idp,
            IEnumerable<string> authenticationMethods,
            params Claim[] claims)
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException(nameof(subject));
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (authenticationMethods.IsNullOrEmpty()) throw new ArgumentNullException(nameof(authenticationMethods));
            if (String.IsNullOrWhiteSpace(idp)) throw new ArgumentNullException(nameof(idp));
            
            var allClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, subject),
                new Claim(JwtClaimTypes.Name, name),
                new Claim(JwtClaimTypes.IdentityProvider, idp),
                new Claim(JwtClaimTypes.AuthenticationTime, DateTimeHelper.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer)
            };

            foreach (var amr in authenticationMethods)
            {
                allClaims.Add(new Claim(JwtClaimTypes.AuthenticationMethod, amr));
            }

            // todo: filtering?
            foreach (var claim in claims)
            {
                allClaims.Add(claim);
            }

            var id = new ClaimsIdentity(allClaims.Distinct(new ClaimComparer()), Constants.IdentityServerAuthenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }

        internal static void AssertRequiredClaims(this ClaimsPrincipal principal)
        {
            // todo: multi accounts?
            if (principal.Identities.Count() != 1) throw new InvalidOperationException("only a single identity supported");
            if (principal.FindFirst(JwtClaimTypes.Subject) == null) throw new InvalidOperationException("sub claim is missing");
            if (principal.FindFirst(JwtClaimTypes.Name) == null) throw new InvalidOperationException("name claim is missing");
        }

        internal static void AugmentMissingClaims(this ClaimsPrincipal principal)
        {
            if (principal.FindFirst(JwtClaimTypes.IdentityProvider) == null)
            {
                principal.Identities.First().AddClaim(new Claim(JwtClaimTypes.IdentityProvider, Constants.LocalIdentityProvider));
            }

            if (principal.FindFirst(JwtClaimTypes.AuthenticationMethod) == null)
            {
                principal.Identities.First().AddClaim(new Claim(JwtClaimTypes.AuthenticationMethod, OidcConstants.AuthenticationMethods.Password));
            }

            if (principal.FindFirst(JwtClaimTypes.AuthenticationTime) == null)
            {
                principal.Identities.First().AddClaim(new Claim(JwtClaimTypes.AuthenticationTime, DateTimeHelper.UtcNow.ToEpochTime().ToString(), ClaimValueTypes.Integer));
            }
        }

        //public static ClaimsPrincipal CreateFromPrincipal(ClaimsPrincipal principal, string authenticationType)
        //{
        //    // we require the following claims
        //    var subject = principal.FindFirst(JwtClaimTypes.Subject);
        //    if (subject == null) throw new InvalidOperationException("sub claim is missing");

        //    var name = principal.FindFirst(JwtClaimTypes.Name);
        //    if (name == null) throw new InvalidOperationException("name claim is missing");

        //    var authenticationMethod = principal.FindFirst(JwtClaimTypes.AuthenticationMethod);
        //    if (authenticationMethod == null) throw new InvalidOperationException("amr claim is missing");

        //    var authenticationTime = principal.FindFirst(JwtClaimTypes.AuthenticationTime);
        //    if (authenticationTime == null) throw new InvalidOperationException("auth_time claim is missing");

        //    var idp = principal.FindFirst(JwtClaimTypes.IdentityProvider);
        //    if (idp == null) throw new InvalidOperationException("idp claim is missing");

        //    var id = new ClaimsIdentity(principal.Claims, authenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
        //    return new ClaimsPrincipal(id);
        //}

        internal static ClaimsPrincipal FromSubjectId(string subjectId, IEnumerable<Claim> additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, subjectId)
            };

            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            return Principal.Create(Constants.IdentityServerAuthenticationType,
                claims.Distinct(new ClaimComparer()).ToArray());
        }

        //public static ClaimsPrincipal FromClaims(IEnumerable<Claim> claims, bool allowMissing = false)
        //{
        //    var amr = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.AuthenticationMethod);
        //    var sub = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);
        //    var idp = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.IdentityProvider);
        //    var authTime = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.AuthenticationTime);

        //    var id = new ClaimsIdentity(Constants.DefaultAuthenticationType);

        //    if (sub != null)
        //    {
        //        id.AddClaim(sub);
        //    }
        //    else
        //    {
        //        if (allowMissing == false)
        //        {
        //            throw new InvalidOperationException("sub claim is missing");
        //        }
        //    }

        //    if (amr != null)
        //    {
        //        id.AddClaim(amr);
        //    }
        //    else
        //    {
        //        if (allowMissing == false)
        //        {
        //            throw new InvalidOperationException("amr claim is missing");
        //        }
        //    }

        //    if (idp != null)
        //    {
        //        id.AddClaim(idp);
        //    }
        //    else
        //    {
        //        if (allowMissing == false)
        //        {
        //            throw new InvalidOperationException("idp claim is missing");
        //        }
        //    }

        //    if (authTime != null)
        //    {
        //        id.AddClaim(authTime);
        //    }
        //    else
        //    {
        //        if (allowMissing == false)
        //        {
        //            throw new InvalidOperationException("auth_time claim is missing");
        //        }
        //    }

        //    return new ClaimsPrincipal(id);
        //}
    }
}