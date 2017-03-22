
namespace IdentityServer4.Events
{
    public class ApiAuthenticationSuccessEvent : Event
    {
        public ApiAuthenticationSuccessEvent(string apiName, string authenticationMethod)
            : base(EventCategories.Authentication, 
                  "API Authentication Success",
                  EventTypes.Success, 
                  EventIds.ApiAuthenticationSuccess)
        {
            ApiName = apiName;
            AuthenticationMethod = authenticationMethod;
        }

        public string ApiName { get; set; }
        public string AuthenticationMethod { get; set; }
    }
}
