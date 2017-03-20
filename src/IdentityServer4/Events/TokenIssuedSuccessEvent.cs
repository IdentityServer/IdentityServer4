
namespace IdentityServer4.Events
{
    public class TokenIssuedSuccessEvent : Event
    {
        public TokenIssuedSuccessEvent()
            : base(EventCategories.Token, 
                  "Token Issued Success",
                  EventTypes.Success, 
                  EventIds.TokenIssuedSuccess)
        {
        }

    }
}
