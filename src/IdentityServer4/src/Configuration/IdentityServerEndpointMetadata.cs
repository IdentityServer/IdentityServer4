namespace IdentityServer4.Configuration
{
    internal class IdentityServerEndpointMetadata
    {
        public IdentityServerEndpointMetadata(Hosting.Endpoint endpoint)
        {
            Endpoint = endpoint;
        }

        public Hosting.Endpoint Endpoint { get; }
    }
}
