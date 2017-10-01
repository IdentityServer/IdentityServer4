using IdentityServer4.Models;

namespace IdentityServer4.Services
{
    public interface IClientSubjectService
    {
        string CreateSubject(string userSubject, Client client);
        bool ValidateSubject(string userSubject, string tokenSubject, Client client);
    }
}