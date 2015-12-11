using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using System;

namespace IdentityServer4.Core.Hosting
{
    public static class IdentityServerContextExtensions
    {
        public static void SetHost(this IdentityServerContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.HttpContext.Items[Constants.OwinEnvironment.IdentityServerHost] = value;
        }

        public static void SetBasePath(this IdentityServerContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.HttpContext.Items[Constants.OwinEnvironment.IdentityServerBasePath] = value;
        }

        public static string GetHost(this IdentityServerContext context)
        {
            return context.HttpContext.Items[Constants.OwinEnvironment.IdentityServerHost] as string;
        }

        /// <summary>
        /// Gets the base path of IdentityServer. Can be used inside of Katana <c>Map</c>ped middleware.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetBasePath(this IdentityServerContext context)
        {
            return context.HttpContext.Items[Constants.OwinEnvironment.IdentityServerBasePath] as string;
        }

        /// <summary>
        /// Gets the public base URL for IdentityServer.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetIdentityServerBaseUrl(this IdentityServerContext context)
        {
            return context.GetHost() + context.GetBasePath();
        }

        public static string GetIssuerUri(this IdentityServerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            //var options = context.HttpContext.ApplicationServices.GetService(typeof(IdentityServerOptions)) as IdentityServerOptions;

            // if they've explicitly configured a URI then use it,
            // otherwise dynamically calculate it
            var uri = context.Options.IssuerUri;
            if (uri.IsMissing())
            {
                uri = context.GetIdentityServerBaseUrl();
                if (uri.EndsWith("/")) uri = uri.Substring(0, uri.Length - 1);
            }

            return uri;
        }
    }
}