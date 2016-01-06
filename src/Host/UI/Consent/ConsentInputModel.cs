using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Host.UI.Consent
{
    public class ConsentInputModel
    {
        public string ConsentId { get; set; }
        public string[] ScopesConsented { get; set; }
        public bool RememberConsent { get; set; }
    }
}
