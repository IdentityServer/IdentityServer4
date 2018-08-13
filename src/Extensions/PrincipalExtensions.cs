// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace IdentityServer4.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="System.Security.Principal.IPrincipal"/> and <see cref="System.Security.Principal.IIdentity"/> .
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Gets the authentication time.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DateTime GetAuthenticationTime(this IPrincipal principal)
        {
            return ((int)principal.GetAuthenticationTimeEpoch()).ToDateTimeFromEpoch();
        }

        /// <summary>
        /// Gets the authentication epoch time.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationTimeEpoch();
        }

        /// <summary>
        /// Gets the authentication epoch time.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetAuthenticationTimeEpoch(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(JwtClaimTypes.AuthenticationTime);

            if (claim == null) throw new InvalidOperationException("auth_time is missing.");
           
            return long.Parse(claim.Value);
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IPrincipal principal)
        {
            return principal.Identity.GetSubjectId();
        }

        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">sub claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetSubjectId(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(JwtClaimTypes.Subject);

            if (claim == null) throw new InvalidOperationException("sub claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [Obsolete("This method will be removed in a future version. Use GetDisplayName instead.")]
        public static string GetName(this IPrincipal principal)
        {
            return principal.Identity.GetName();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetDisplayName(this ClaimsPrincipal principal)
        {
            var name = principal.Identity.Name;
            if (name.IsPresent()) return name;

            var sub = principal.FindFirst(JwtClaimTypes.Subject);
            if (sub != null) return sub.Value;

            return "";
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">name claim is missing</exception>
        [DebuggerStepThrough]
        [Obsolete("This method will be removed in a future version. Use GetDisplayName instead.")]
        public static string GetName(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(JwtClaimTypes.Name);

            if (claim == null) throw new InvalidOperationException("name claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationMethod();
        }

        /// <summary>
        /// Gets the authentication method claims.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<Claim> GetAuthenticationMethods(this IPrincipal principal)
        {
            return principal.Identity.GetAuthenticationMethods();
        }

        /// <summary>
        /// Gets the authentication method.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">amr claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetAuthenticationMethod(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(JwtClaimTypes.AuthenticationMethod);

            if (claim == null) throw new InvalidOperationException("amr claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Gets the authentication method claims.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<Claim> GetAuthenticationMethods(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            return id.FindAll(JwtClaimTypes.AuthenticationMethod);
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IPrincipal principal)
        {
            return principal.Identity.GetIdentityProvider();
        }

        /// <summary>
        /// Gets the identity provider.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">idp claim is missing</exception>
        [DebuggerStepThrough]
        public static string GetIdentityProvider(this IIdentity identity)
        {
            var id = identity as ClaimsIdentity;
            var claim = id.FindFirst(JwtClaimTypes.IdentityProvider);

            if (claim == null) throw new InvalidOperationException("idp claim is missing");
            return claim.Value;
        }

        /// <summary>
        /// Determines whether this instance is authenticated.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns>
        ///   <c>true</c> if the specified principal is authenticated; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public static bool IsAuthenticated(this IPrincipal principal)
        {
            return principal != null && principal.Identity != null && principal.Identity.IsAuthenticated;
        }
    }
}