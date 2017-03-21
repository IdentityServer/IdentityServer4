
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using System.Collections.Generic;

namespace IdentityServer4.Events
{
    public class TokenIssuedSuccessEvent : Event
    {
        public TokenIssuedSuccessEvent(AuthorizeResponse response)
            : this()
        {
            ClientId = response.Request.ClientId;
            ClientName = response.Request.Client.ClientName;
            RedirectUri = response.RedirectUri;
            Endpoint = EndpointName.Authorize.ToString();
            SubjectId = response.Request.Subject.GetSubjectId();
            Scopes = response.Scope;
            GrantType = response.Request.GrantType;

            var tokens = new List<Token>();
            if (response.IdentityToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.IdentityToken, response.IdentityToken));
            }
            if (response.Code != null)
            {
                tokens.Add(new Token(OidcConstants.ResponseTypes.Code, response.Code));
            }
            if (response.AccessToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.AccessToken, response.AccessToken));
            }
            Tokens = tokens;
        }

        public TokenIssuedSuccessEvent(TokenResponse response, TokenRequestValidationResult request)
            : this()
        {
            ClientId = request.ValidatedRequest.Client.ClientId;
            ClientName = request.ValidatedRequest.Client.ClientName;
            Endpoint = EndpointName.Token.ToString();
            SubjectId = request.ValidatedRequest.Subject?.GetSubjectId();
            GrantType = request.ValidatedRequest.GrantType;

            if (GrantType == OidcConstants.GrantTypes.RefreshToken)
            {
                Scopes = request.ValidatedRequest.RefreshToken.AccessToken.Scopes.ToSpaceSeparatedString();
            }
            else if (GrantType == OidcConstants.GrantTypes.AuthorizationCode)
            {
                Scopes = request.ValidatedRequest.AuthorizationCode.RequestedScopes.ToSpaceSeparatedString();
            }
            else
            {
                Scopes = request.ValidatedRequest.ValidatedScopes?.GrantedResources.ToScopeNames().ToSpaceSeparatedString();
            }

            var tokens = new List<Token>();
            if (response.IdentityToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.IdentityToken, response.IdentityToken));
            }
            if (response.RefreshToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.RefreshToken, response.RefreshToken));
            }
            if (response.AccessToken != null)
            {
                tokens.Add(new Token(OidcConstants.TokenTypes.AccessToken, response.AccessToken));
            }
            Tokens = tokens;
        }

        protected TokenIssuedSuccessEvent()
            : base(EventCategories.Token,
                  "Token Issued Success",
                  EventTypes.Success,
                  EventIds.TokenIssuedSuccess)
        {
        }

        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string RedirectUri { get; set; }
        public string Endpoint { get; set; }
        public string SubjectId { get; set; }
        public string Scopes { get; set; }
        public string GrantType { get; set; }
        public IEnumerable<Token> Tokens { get; set; }

        public class Token
        {
            public Token(string type, string value)
            {
                TokenType = type;
                TokenValue = ObfuscateToken(value);
            }

            public string TokenType { get; }
            public string TokenValue { get; }
        }
    }
}
