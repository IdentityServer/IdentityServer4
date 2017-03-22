
namespace IdentityServer4.Events
{
    public class ClientAuthenticationSuccessEvent : Event
    {
        public ClientAuthenticationSuccessEvent(string clientId, string authenticationMethod)
            : base(EventCategories.Authentication, 
                  "Client Authentication Success",
                  EventTypes.Success, 
                  EventIds.ClientAuthenticationSuccess)
        {
            ClientId = clientId;
            AuthenticationMethod = authenticationMethod;
        }

        public string ClientId { get; set; }
        public string AuthenticationMethod { get; set; }
    }
}
