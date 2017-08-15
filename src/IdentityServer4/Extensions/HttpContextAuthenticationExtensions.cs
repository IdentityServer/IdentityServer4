﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Extension methods for signin/out using the IdentityServer authentication scheme.
    /// </summary>
    public static class AuthenticationManagerExtensions
    {
        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="name">The name.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, string subject, string name, params Claim[] claims)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var principal = IdentityServerPrincipal.Create(subject, name, options.UtcNow, claims);
            await context.SignInAsync(scheme, principal);
        }

        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="name">The name.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, string subject, string name, AuthenticationProperties properties, params Claim[] claims)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var principal = IdentityServerPrincipal.Create(subject, name, options.UtcNow, claims);
            await context.SignInAsync(scheme, principal, properties);
        }

        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="name">The name.</param>
        /// <param name="identityProvider">The identity provider.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, string subject, string name, string identityProvider, params Claim[] claims)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var principal = IdentityServerPrincipal.Create(subject, name, identityProvider, options.UtcNow, claims);
            await context.SignInAsync(scheme, principal);
        }

        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="name">The name.</param>
        /// <param name="identityProvider">The identity provider.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, string subject, string name, string identityProvider, AuthenticationProperties properties, params Claim[] claims)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var principal = IdentityServerPrincipal.Create(subject, name, identityProvider, options.UtcNow, claims);
            await context.SignInAsync(scheme, principal, properties);
        }

        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="name">The name.</param>
        /// <param name="authenticationMethods">The authentication methods.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, string subject, string name, IEnumerable<string> authenticationMethods, params Claim[] claims)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var principal = IdentityServerPrincipal.Create(subject, name, authenticationMethods, options.UtcNow, claims);
            await context.SignInAsync(scheme, principal);
        }

        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="name">The name.</param>
        /// <param name="authenticationMethods">The authentication methods.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, string subject, string name, IEnumerable<string> authenticationMethods, AuthenticationProperties properties, params Claim[] claims)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var principal = IdentityServerPrincipal.Create(subject, name, authenticationMethods, options.UtcNow, claims);
            await context.SignInAsync(scheme, principal, properties);
        }

        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="sub">The sub.</param>
        /// <param name="name">The name.</param>
        /// <param name="identityProvider">The identity provider.</param>
        /// <param name="authenticationMethods">The authentication methods.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, string sub, string name, string identityProvider, IEnumerable<string> authenticationMethods, params Claim[] claims)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var principal = IdentityServerPrincipal.Create(sub, name, identityProvider, authenticationMethods, options.UtcNow, claims);
            await context.SignInAsync(scheme, principal);
        }

        /// <summary>
        /// Signs the user in.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <param name="sub">The sub.</param>
        /// <param name="name">The name.</param>
        /// <param name="identityProvider">The identity provider.</param>
        /// <param name="authenticationMethods">The authentication methods.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        public static async Task SignInAsync(this HttpContext context, string sub, string name, string identityProvider, IEnumerable<string> authenticationMethods, AuthenticationProperties properties, params Claim[] claims)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var principal = IdentityServerPrincipal.Create(sub, name, identityProvider, authenticationMethods, options.UtcNow, claims);
            await context.SignInAsync(scheme, principal, properties);
        }

        /// <summary>
        /// Signs the user out.
        /// </summary>
        /// <param name="context">The manager.</param>
        /// <returns></returns>
        public static async Task SignOut2Async(this HttpContext context)
        {
            var scheme = context.GetIdentityServerAuthenticationScheme();
            await context.SignOutAsync(scheme);
        }

        /// <summary>
        /// Gets the IdentityServer authentication scheme.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <returns></returns>
        public static string GetIdentityServerAuthenticationScheme(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<IdentityServerOptions>().Authentication.EffectiveAuthenticationScheme;
        }
    }
}