namespace IdentityServer4.Services
{
    public class DeviceFlowInteractionResult
    {
        public string ErrorDescription { get; set; }
        public bool IsError { get; private set; }

        public bool IsConsent { get; set; }
        public bool IsAccessDenied { get; set; }

        public static DeviceFlowInteractionResult Failure(string errorDescription = null)
        {
            return new DeviceFlowInteractionResult
            {
                IsError = true,
            };
        }
    }
}