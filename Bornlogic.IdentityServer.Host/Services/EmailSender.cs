using Microsoft.AspNetCore.Identity.UI.Services;

namespace Bornlogic.IdentityServer.Host.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            //TODO: USE MANDRILL
            return Task.CompletedTask;
        }
        
    }
}
