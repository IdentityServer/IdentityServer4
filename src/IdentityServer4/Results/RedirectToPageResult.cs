using System;
using System.Threading.Tasks;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Extensions;

namespace IdentityServer4.Core.Results
{
    public class RedirectToPageResult : IEndpointResult
    {
        public string Path { get; set; }
        public string Id { get; set; }

        public RedirectToPageResult(string path)
            : this(path, null)
        {
        }

        public RedirectToPageResult(string path, string id)
        {
            Path = path;
            Id = id;
        }

        public Task ExecuteAsync(IdentityServerContext context)
        {
            var redirect = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + Path.RemoveLeadingSlash();
            if (Id.IsPresent())
            {
                redirect = redirect.AddQueryString("id=" + Id);
            }

            context.HttpContext.Response.Redirect(redirect);

            return Task.FromResult(0);
        }
    }
}
