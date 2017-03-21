
namespace IdentityServer4.Events
{
    public class ClientAuthenticationFailureEvent : Event
    {
        public ClientAuthenticationFailureEvent(string clientId, string message)
            : base(EventCategories.ClientAuthentication, 
                  "Client Authentication Success",
                  EventTypes.Success, 
                  EventIds.ClientAuthenticationSuccess, 
                  message)
        {
            ClientId = clientId;
        }

        public string ClientId { get; set; }
    }
}
