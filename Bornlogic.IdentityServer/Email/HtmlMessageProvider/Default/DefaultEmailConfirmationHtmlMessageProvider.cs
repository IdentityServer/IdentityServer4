using System.Text.Encodings.Web;
using Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts;

namespace Bornlogic.IdentityServer.Email.HtmlMessageProvider.Default
{
    public class DefaultEmailConfirmationProvider : IEmailConfirmationProvider
    {
        public Task<KeyValuePair<string, string>> GetSubjectAndHtmlMessage(string userName, string callbackUrl)
        {
            return Task.FromResult(new KeyValuePair<string, string>("Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."));
        }
    }
}
