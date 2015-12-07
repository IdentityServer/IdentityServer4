namespace IdentityServer4.Core.Models
{
    public class JsonWebKey
    {
        public string kty { get; set; }
        public string use { get; set; }
        public string kid { get; set; }
        public string x5t { get; set; }
        public string e { get; set; }
        public string n { get; set; }
        public string[] x5c { get; set; }
    }
}