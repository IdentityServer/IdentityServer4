
using IdentityServer4.Hosting;

namespace IdentityServer4.Events
{
    public class UserLoginSuccessEvent : Event
    {
        public UserLoginSuccessEvent(string provider, string providerUserId, string subjectId, string name)
            : base(EventCategories.UserAuthentication,
                  "User Login Success",
                  EventTypes.Success,
                  EventIds.UserLoginSuccess)
        {
            Provider = provider;
            ProviderUserId = providerUserId;
            SubjectId = subjectId;
            DisplayName = name;
            Endpoint = "UI";
        }

        public UserLoginSuccessEvent(string username, string subjectId, string name, bool interactive = true)
            : base(EventCategories.UserAuthentication,
                  "User Login Success",
                  EventTypes.Success,
                  EventIds.UserLoginSuccess)
        {
            Username = username;
            SubjectId = subjectId;
            DisplayName = name;

            if (interactive)
            {
                Endpoint = "UI";
            }
            else
            {
                Endpoint = EndpointName.Token.ToString();
            }
        }

        public string Username { get; set; }
        public string Provider { get; set; }
        public string ProviderUserId { get; set; }
        public string SubjectId { get; set; }
        public string DisplayName { get; set; }
        public string Endpoint { get; set; }
    }
}
