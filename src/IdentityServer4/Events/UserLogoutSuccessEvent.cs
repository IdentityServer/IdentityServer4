
namespace IdentityServer4.Events
{
    public class UserLogoutSuccessEvent : Event
    {
        public UserLogoutSuccessEvent(string subjectId, string name)
            : base(EventCategories.Authentication, 
                  "User Logout Success",
                  EventTypes.Success, 
                  EventIds.UserLogoutSuccess)
        {
            SubjectId = subjectId;
            DisplayName = name;
        }

        public string SubjectId { get; set; }
        public string DisplayName { get; set; }
    }
}
