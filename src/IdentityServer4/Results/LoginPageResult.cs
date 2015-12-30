using System.Threading.Tasks;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Extensions;

namespace IdentityServer4.Core.Results
{
    public class LoginPageResult : IEndpointResult
    {
        public string Id { get; set; }

        public LoginPageResult(string id)
        {
            Id = id;
        }

        public Task ExecuteAsync(IdentityServerContext context)
        {
            var url = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Constants.RoutePaths.Login;
            url = url.AddQueryString("id=" + Id);

            context.HttpContext.Response.Redirect(url);

            return Task.FromResult(0);
        }
    }
}
