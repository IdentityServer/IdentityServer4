using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Hosting.LocalApiAuthentication
{
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

    public class ClaimsTransformationContext
    {
        public ClaimsPrincipal Principal { get; set; }
        public HttpContext HttpContext { get; set; }
    }
}