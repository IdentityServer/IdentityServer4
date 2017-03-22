
namespace IdentityServer4.Events
{
    public class UserLoginFailureEvent : Event
    {
        public UserLoginFailureEvent(string username, string error)
            : base(EventCategories.Authentication,
                  "User Login Failure",
                  EventTypes.Failure, 
                  EventIds.UserLoginFailure,
                  error)
        {
            Username = username;
        }

        public string Username { get; set; }
    }
}
