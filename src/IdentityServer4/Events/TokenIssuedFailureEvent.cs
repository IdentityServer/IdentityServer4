
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer4.Events
{
    public class TokenIssuedFailureEvent : Event
    {
        public TokenIssuedFailureEvent(ValidatedAuthorizeRequest request, string error, string description)
            : this()
        {
            if (request != null)
            {
                ClientId = request.ClientId;
                ClientName = request.Client?.ClientName;
                RedirectUri = request.RedirectUri;
                Scopes = request.RequestedScopes?.ToSpaceSeparatedString();
                GrantType = request.GrantType;

                if (request.Subject != null && request.Subject.Identity.IsAuthenticated)
                {
                    SubjectId = request.Subject?.GetSubjectId();
                }
            }

            Endpoint = EndpointName.Authorize.ToString();
            Error = error;
            ErrorDescription = description;
        }

        public TokenIssuedFailureEvent(TokenRequestValidationResult result)
            : this()
        {
            if (result.ValidatedRequest != null)
            {
                ClientId = result.ValidatedRequest.Client.ClientId;
                ClientName = result.ValidatedRequest.Client.ClientName;
                GrantType = result.ValidatedRequest.GrantType;
                Scopes = result.ValidatedRequest.Scopes?.ToSpaceSeparatedString();

                if (result.ValidatedRequest.Subject != null && result.ValidatedRequest.Subject.Identity.IsAuthenticated)
                {
                    SubjectId = result.ValidatedRequest.Subject.GetSubjectId();
                }
            }

            Endpoint = EndpointName.Token.ToString();
            Error = result.Error;
            ErrorDescription = result.ErrorDescription;
        }

        protected TokenIssuedFailureEvent()
            : base(EventCategories.Token,
                  "Token Issued Failure",
                  EventTypes.Failure,
                  EventIds.TokenIssuedFailure)
        {
        }

        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string RedirectUri { get; set; }
        public string Endpoint { get; set; }
        public string SubjectId { get; set; }
        public string Scopes { get; set; }
        public string GrantType { get; set; }
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
}
