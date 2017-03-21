using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer4.Events
{
    public class TokenRevokedSuccessEvent : Event
    {
        public TokenRevokedSuccessEvent(TokenRevocationRequestValidationResult requestResult, Client client)
            : base(EventCategories.Token,
                  "Token Revoked Success",
                  EventTypes.Success,
                  EventIds.TokenRevokedSuccess)
        {
            ClientId = client.ClientId;
            ClientName = client.ClientName;
            TokenType = requestResult.TokenTypeHint;
            Token = ObfuscateToken(requestResult.Token);
        }

        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string TokenType { get; set; }
        public string Token { get; set; }
    }
}
