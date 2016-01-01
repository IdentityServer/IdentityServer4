using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Tests.Common
{
    // thanks to Damian Hickey for this awesome sample
    // https://github.com/damianh/OwinHttpMessageHandler/blob/master/src/OwinHttpMessageHandler/OwinHttpMessageHandler.cs
    // but the name "BrowsR" is ours
    public class BrowsR : DelegatingHandler
    {
        private CookieContainer _cookieContainer = new CookieContainer();

        public bool AllowAutoRedirect { get; set; } = true;
        public bool AllowCookies { get; set; } = true;
        public int AutoRedirectLimit { get; set; } = 20;

        public BrowsR(HttpMessageHandler next)
            : base(next)
        {
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await SendCookiesAsync(request, cancellationToken);

            int redirectCount = 0;

            while (AllowAutoRedirect && (
                    response.StatusCode == HttpStatusCode.Moved
                    || response.StatusCode == HttpStatusCode.Found))
            {
                if (redirectCount >= AutoRedirectLimit)
                {
                    throw new InvalidOperationException(string.Format("Too many redirects. Limit = {0}", redirectCount));
                }
                var location = response.Headers.Location;
                if (!location.IsAbsoluteUri)
                {
                    location = new Uri(response.RequestMessage.RequestUri, location);
                }

                request = new HttpRequestMessage(HttpMethod.Get, location);

                response = await SendCookiesAsync(request, cancellationToken).ConfigureAwait(false);

                redirectCount++;
            }
            return response;
        }

        protected async Task<HttpResponseMessage> SendCookiesAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (AllowCookies)
            {
                string cookieHeader = _cookieContainer.GetCookieHeader(request.RequestUri);
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    request.Headers.Add("Cookie", cookieHeader);
                }
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (AllowCookies && response.Headers.Contains("Set-Cookie"))
            {
                var responseCookieHeader = string.Join(",", response.Headers.GetValues("Set-Cookie"));
                _cookieContainer.SetCookies(request.RequestUri, responseCookieHeader);
            }

            return response;
        }
    }
}
