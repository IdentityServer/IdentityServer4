
namespace IdentityServer4.Events
{
    public class UserLogoutSuccessEvent : Event
    {
        public UserLogoutSuccessEvent(string subjectId)
            : base(EventCategories.UserAuthentication, 
                  "User Logout Success",
                  EventTypes.Success, 
                  EventIds.UserLogoutSuccess)
        {
            SubjectId = subjectId;
        }

        public string SubjectId { get; set; }
    }
}
