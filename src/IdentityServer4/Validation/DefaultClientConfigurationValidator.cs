using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default client configuration validator
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.IClientConfigurationValidator" />
    public class DefaultClientConfigurationValidator : IClientConfigurationValidator
    {
        public async Task ValidateAsync(ClientConfigurationValidationContext context)
        {
            await ValidateLifetimesAsync(context);
            if (context.IsValid == false) return;

            await ValidatePropertiesAsync(context);
            if (context.IsValid == false) return;
        }

        protected Task ValidateLifetimesAsync(ClientConfigurationValidationContext context)
        {
            if (context.Client.AccessTokenLifetime <= 0)
            {
                context.SetError("access token lifetime is 0 or negative");
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        protected Task ValidatePropertiesAsync(ClientConfigurationValidationContext context)
        {
            return Task.CompletedTask;
        }
    }
}