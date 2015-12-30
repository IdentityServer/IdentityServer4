using Microsoft.AspNet.Http;

namespace IdentityServer4.Core.Hosting
{
    public interface IEndpointRouter
    {
        // TODO: does this need to be async?
        IEndpoint Find(HttpContext context);
    }
}
