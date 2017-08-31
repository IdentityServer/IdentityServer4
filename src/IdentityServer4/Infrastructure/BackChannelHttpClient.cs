using System.Net.Http;

namespace IdentityServer4.Infrastructure
{
    /// <summary>
    /// Used to model back-channel HTTP calls for back-channel logout spec.
    /// </summary>
    /// <seealso cref="System.Net.Http.HttpClient" />
    public class BackChannelHttpClient : HttpClient
    {
        public BackChannelHttpClient()
        {
        }

        public BackChannelHttpClient(HttpMessageHandler handler) : base(handler)
        {
        }
    }
}
