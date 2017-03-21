
namespace IdentityServer4.Events
{
    public class ApiAuthenticationFailureEvent : Event
    {
        public ApiAuthenticationFailureEvent(string apiName, string message)
            : base(EventCategories.ApiAuthentication, 
                  "API Authentication Success",
                  EventTypes.Success, 
                  EventIds.ApiAuthenticationSuccess, 
                  message)
        {
            ApiName = apiName;
        }

        public string ApiName { get; set; }
    }
}
