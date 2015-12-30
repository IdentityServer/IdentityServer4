using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Extensions;
using Microsoft.Extensions.WebEncoders;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    class AuthorizeFormPostResult : AuthorizeResult
    {
        private readonly IHtmlEncoder _encoder;

        public AuthorizeFormPostResult(AuthorizeResponse response, IHtmlEncoder encoder)
            : base(response)
        {
            _encoder = encoder;
        }

        internal static string BuildFormBody(AuthorizeResponse response, IHtmlEncoder encoder)
        {
            return response.ToNameValueCollection().ToFormPost(encoder);
        }

        public override Task ExecuteAsync(IdentityServerContext context)
        {
            return Task.FromResult(0);
        }
    }
}
