using IdentityServer4.Models;

namespace IdentityServer4.Services.Default
{
    /// <summary>
    /// Default pair wise subject service
    /// </summary>
    public class DefaultPairWiseSubjectService : IPairWiseSubjectService
    {
        /// <summary>
        /// Returns pair wise subject for a user
        /// </summary>
        /// <param name="subject">The original subject string.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public string GetPairWiseSubject(string subject, Client client)
        {
            if (string.IsNullOrWhiteSpace(client.PairWiseSubjectSalt))
            {
                return subject;
            }

            return $"{subject}{client.PairWiseSubjectSalt}".Sha256();
        }
    }
}