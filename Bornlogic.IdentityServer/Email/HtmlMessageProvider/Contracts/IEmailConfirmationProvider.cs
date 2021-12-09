namespace Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts
{
    public interface IEmailConfirmationProvider
    {
        Task<KeyValuePair<string, string>> GetSubjectAndHtmlMessage(string userName, string callbackUrl);
    }
}
