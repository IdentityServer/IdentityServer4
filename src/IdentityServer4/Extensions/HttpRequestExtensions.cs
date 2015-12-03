using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.AspNet.Http
{
    public static class HttpRequestExtensions
    {
        public static NameValueCollection AsNameValueCollection(this IFormCollection form)
        {
            var nv = new NameValueCollection();

            foreach (var field in form)
            {
                nv.Add(field.Key, field.Value.First());
            }

            return nv;
        }
    }
}