using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Endpoints.Results
{
    class EndSessionCallbackResult : HtmlPageResult
    {
        private IEnumerable<string> _urls;

        public EndSessionCallbackResult(IEnumerable<string> urls)
        {
            this._urls = urls;
        }

        protected override string GetHtml()
        {
            string framesHtml = null;

            if (_urls != null && _urls.Any())
            {
                var frameUrls = _urls.Select(x => $"<iframe src='{x}'></iframe>");
                framesHtml = frameUrls.Aggregate((x, y) => x + y);
            }

            return $"<!DOCTYPE html><html><body>{framesHtml}</body></html>";
        }
    }
}
