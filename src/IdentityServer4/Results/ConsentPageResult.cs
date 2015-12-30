using System.Threading.Tasks;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Extensions;

namespace IdentityServer4.Core.Results
{
    public class ConsentPageResult : IEndpointResult
    {
        public string Id { get; set; }

        public ConsentPageResult(string id)
        {
            Id = id;
        }

        public Task ExecuteAsync(IdentityServerContext context)
        {
            var url = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Constants.RoutePaths.Oidc.Consent;
            url = url.AddQueryString("id=" + Id);

            context.HttpContext.Response.Redirect(url);

            return Task.FromResult(0);
        }
    }
}
