using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;

namespace Host.UI.Consent
{
    public class ConsentViewModel : ConsentInputModel
    {
        public ConsentViewModel(string consentId, ConsentRequest request, Client client, IEnumerable<Scope> scopes)
        {
            ConsentId = consentId;

            ClientName = client.ClientName;
            ClientUrl = client.ClientUri;
            ClientLogoUrl = client.LogoUri;
            AllowRememberConsent = client.AllowRememberConsent;

            IdentityScopes = scopes.Where(x => x.Type == ScopeType.Identity).Select(x => new ScopeViewModel(x, false));
            ResourceScopes = scopes.Where(x => x.Type == ScopeType.Resource).Select(x => new ScopeViewModel(x, false));
        }

        public string ClientName { get; set; }
        public string ClientUrl { get; set; }
        public string ClientLogoUrl { get; set; }
        public bool AllowRememberConsent { get; set; }

        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
        public IEnumerable<ScopeViewModel> ResourceScopes { get; set; }
    }

    public class ScopeViewModel
    {
        public ScopeViewModel(Scope scope, bool check)
        {
            Name = scope.Name;
            DisplayName = scope.DisplayName;
            Description = scope.Description;
            Emphasize = scope.Emphasize;
            Required = scope.Required;
            Checked = check || scope.Required;
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Emphasize { get; set; }
        public bool Required { get; set; }
        public bool Checked { get; set; }
    }
}
