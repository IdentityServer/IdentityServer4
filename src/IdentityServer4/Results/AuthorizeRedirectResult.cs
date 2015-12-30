using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Extensions;
using Microsoft.Extensions.WebEncoders;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    class AuthorizeRedirectResult : AuthorizeResult
    {
        private readonly IUrlEncoder _encoder;

        public AuthorizeRedirectResult(AuthorizeResponse response, IUrlEncoder urlEncoder)
            : base(response)
        {
            _encoder = urlEncoder;
        }

        internal static string BuildUri(AuthorizeResponse response, IUrlEncoder encoder)
        {
            var uri = response.RedirectUri;
            var query = response.ToNameValueCollection().ToQueryString(encoder);

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

        public override Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.Redirect(BuildUri(Response, _encoder));
            return Task.FromResult(0);
        }
    }
}
