
namespace IdentityServer4.Events
{
    public class ClientAuthenticationFailureEvent : Event
    {
        public ClientAuthenticationFailureEvent(string clientId, string message)
            : base(EventCategories.Authentication, 
                  "Client Authentication Failure",
                  EventTypes.Failure, 
                  EventIds.ClientAuthenticationFailure, 
                  message)
        {
            ClientId = clientId;
        }

        public string ClientId { get; set; }
    }
}
