﻿using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Bornlogic.IdentityServer.Hosting.LocalApiAuthentication
{
    /// <summary>
    /// Events for local API authentication
    /// </summary>
    public class LocalApiAuthenticationEvents
    {
        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public Func<ClaimsTransformationContext, Task> OnClaimsTransformation { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public virtual Task ClaimsTransformation(ClaimsTransformationContext context) => OnClaimsTransformation(context);

    }

    /// <summary>
    /// Context class for local API claims transformation
    /// </summary>
    public class ClaimsTransformationContext
    {
        /// <summary>
        /// The principal
        /// </summary>
        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        /// the HTTP context
        /// </summary>
        public HttpContext HttpContext { get; internal set; }
    }
}