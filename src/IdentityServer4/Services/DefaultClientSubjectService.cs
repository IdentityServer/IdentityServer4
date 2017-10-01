using System;
using IdentityServer4.Models;

namespace IdentityServer4.Services
{
    public class DefaultClientSubjectService : IClientSubjectService
    {
        public string CreateSubject(string userSubject, Client client)
        {
            if (userSubject == null) throw new ArgumentNullException(nameof(userSubject));
            if (client == null) throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(client.PairWiseSubjectSalt))
            {
                return userSubject;
            }
            return (userSubject + client.PairWiseSubjectSalt).Sha256();
        }

        public bool ValidateSubject(string userSubject, string tokenSubject, Client client)
        {
            if (userSubject == null) throw new ArgumentNullException(nameof(userSubject));
            if (tokenSubject == null) throw new ArgumentNullException(nameof(tokenSubject));
            if (client == null) throw new ArgumentNullException(nameof(client));

            userSubject = CreateSubject(userSubject, client);
            return userSubject == tokenSubject;
        }
    }
}