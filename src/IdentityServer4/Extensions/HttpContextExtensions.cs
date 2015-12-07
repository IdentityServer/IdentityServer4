using IdentityServer4.Core;
using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using System;

namespace Microsoft.AspNet.Http
{
    public static class HttpContextExtensions
    {
        public static void SetIdentityServerHost(this HttpContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Items[Constants.OwinEnvironment.IdentityServerHost] = value;
        }

        public static void SetIdentityServerBasePath(this HttpContext context, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Items[Constants.OwinEnvironment.IdentityServerBasePath] = value;
        }

        public static string GetIdentityServerHost(this HttpContext context)
        {
            return context.Items[Constants.OwinEnvironment.IdentityServerHost] as string;
        }

        /// <summary>
        /// Gets the base path of IdentityServer. Can be used inside of Katana <c>Map</c>ped middleware.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetIdentityServerBasePath(this HttpContext context)
        {
            return context.Items[Constants.OwinEnvironment.IdentityServerBasePath] as string;
        }

        /// <summary>
        /// Gets the public base URL for IdentityServer.
        /// </summary>
        /// <param name="env">The OWIN environment.</param>
        /// <returns></returns>
        public static string GetIdentityServerBaseUrl(this HttpContext context)
        {
            return context.GetIdentityServerHost() + context.GetIdentityServerBasePath();
        }

        public static string GetIdentityServerIssuerUri(this HttpContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var options = context.ApplicationServices.GetService(typeof(IdentityServerOptions)) as IdentityServerOptions;

            // if they've explicitly configured a URI then use it,
            // otherwise dynamically calculate it
            var uri = options.IssuerUri;
            if (uri.IsMissing())
            {
                uri = context.GetIdentityServerBaseUrl();
                if (uri.EndsWith("/")) uri = uri.Substring(0, uri.Length - 1);
            }

            return uri;
        }
    }
}