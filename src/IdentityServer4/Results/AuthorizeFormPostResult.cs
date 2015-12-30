using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Extensions;
using Microsoft.Extensions.WebEncoders;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    class AuthorizeFormPostResult : AuthorizeResult
    {
        public AuthorizeFormPostResult(AuthorizeResponse response)
            : base(response)
        {
        }

        internal static string BuildFormBody(AuthorizeResponse response)
        {
            return response.ToNameValueCollection().ToFormPost();
        }

        public override Task ExecuteAsync(IdentityServerContext context)
        {
            return Task.FromResult(0);
        }
    }
}
