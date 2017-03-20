
namespace IdentityServer4.Events
{
    public class UserLoginSuccessEvent : Event
    {
        public UserLoginSuccessEvent(string username, string subjectId)
            : base(EventCategories.UserAuthentication, 
                  "User Login Success",
                  EventTypes.Success, 
                  EventIds.UserLoginSuccess)
        {
            Username = username;
            SubjectId = subjectId;
        }

        public string Username { get; set; }
        public string SubjectId { get; set; }
    }
}
