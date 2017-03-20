
namespace IdentityServer4.Events
{
    public class TokenIssuedSuccessEvent : Event
    {
        public TokenIssuedSuccessEvent(string grantType, string tokenType)
            : base(EventCategories.Token, 
                  "Token Issued Success",
                  EventTypes.Success, 
                  EventIds.TokenIssuedSuccess)
        {
            GrantType = grantType;
            TokenType = tokenType;
        }

        public string GrantType { get; set; }
        public string TokenType { get; set; }
    }
}
