
namespace IdentityServer4.Events
{
    public class UserLoginSuccessEvent : Event
    {
        public UserLoginSuccessEvent(string username, string subjectId)
            : base(EventConstants.Categories.UserAuthentication, 
                  "User Authentication Success",
                  EventTypes.Success, 
                  EventConstants.Ids.UserLoginSuccess)
        {
            Username = username;
            SubjectId = subjectId;
        }

        public string Username { get; set; }
        public string SubjectId { get; set; }
    }

    public class UserLoginFailureEvent : Event
    {
        public UserLoginFailureEvent(string username, string error)
            : base(EventConstants.Categories.UserAuthentication,
                  "User Authentication Failure",
                  EventTypes.Failure, 
                  EventConstants.Ids.UserLoginFailure,
                  error)
        {
            Username = username;
        }

        public string Username { get; set; }
    }
}
