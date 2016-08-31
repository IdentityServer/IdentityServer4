using System.Collections.Generic;

namespace Host.Models
{
    public class ConsentInputModel
    {
        public IEnumerable<string> ScopesConsented { get; set; }
        public bool RememberConsent { get; set; }
        public string ReturnUrl { get; set; }
    }
}