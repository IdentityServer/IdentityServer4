using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Results
{
    public class LoginPageResult : IResult
    {
        public string Url { get; set; }

        public LoginPageResult(string url)
        {
            Url = url;
        }

        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            context.Response.Redirect(Url);
            return Task.FromResult(0);
        }
    }
}
