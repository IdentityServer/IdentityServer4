using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    public class LoginPageResult : IEndpointResult
    {
        public string Url { get; set; }

        public LoginPageResult(string url)
        {
            Url = url;
        }

        public Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Response.Redirect(Url);
            return Task.FromResult(0);
        }
    }
}
