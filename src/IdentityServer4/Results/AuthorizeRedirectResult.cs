using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Extensions;

namespace IdentityServer4.Core.Results
{
    class AuthorizeRedirectResult : AuthorizeResult
    {
        private readonly AuthorizeResponse _response;

        public AuthorizeRedirectResult(AuthorizeResponse response)
        {
            _response = response;
        }

        internal static string BuildUri(AuthorizeResponse response)
        {
            var uri = response.RedirectUri;
            var query = response.ToNameValueCollection().ToQueryString();

            if (response.Request.ResponseMode == Constants.ResponseModes.Query)
            {
                uri = uri.AddQueryString(query);
            }
            else
            {
                uri = uri.AddHashFragment(query);
            }

            if (response.IsError && !uri.Contains("#"))
            {
                // https://tools.ietf.org/html/draft-bradley-oauth-open-redirector-00
                uri += "#_=_";
            }

            return uri;
        }

        public override Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            context.Response.Redirect(BuildUri(_response));
            return Task.FromResult(0);
        }
    }
}
