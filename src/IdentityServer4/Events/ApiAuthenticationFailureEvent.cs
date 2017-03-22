
namespace IdentityServer4.Events
{
    public class ApiAuthenticationFailureEvent : Event
    {
        public ApiAuthenticationFailureEvent(string apiName, string message)
            : base(EventCategories.Authentication, 
                  "API Authentication Failure",
                  EventTypes.Failure, 
                  EventIds.ApiAuthenticationFailure, 
                  message)
        {
            ApiName = apiName;
        }

        public string ApiName { get; set; }
    }
}
