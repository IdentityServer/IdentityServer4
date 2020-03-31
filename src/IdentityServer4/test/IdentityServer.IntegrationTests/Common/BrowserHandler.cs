﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer.IntegrationTests.Common
{
    // thanks to Damian Hickey for this awesome sample
    // https://github.com/damianh/OwinHttpMessageHandler/blob/master/src/OwinHttpMessageHandler/OwinHttpMessageHandler.cs
    public class BrowserHandler : DelegatingHandler
    {
        private CookieContainer _cookieContainer = new CookieContainer();

        public bool AllowCookies { get; set; } = true;
        public bool AllowAutoRedirect { get; set; } = true;
        public int ErrorRedirectLimit { get; set; } = 20;
        public int StopRedirectingAfter { get; set; } = Int32.MaxValue;

        public BrowserHandler(HttpMessageHandler next)
            : base(next)
        {
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await SendCookiesAsync(request, cancellationToken);

            int redirectCount = 0;

            while (AllowAutoRedirect && 
                (300 <= (int)response.StatusCode && (int)response.StatusCode < 400) &&
                redirectCount < StopRedirectingAfter)
            {
                if (redirectCount >= ErrorRedirectLimit)
                {
                    throw new InvalidOperationException(string.Format("Too many redirects. Error limit = {0}", redirectCount));
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

        internal Cookie GetCookie(string uri, string name)
        {
            return _cookieContainer.GetCookies(new Uri(uri)).Cast<Cookie>().Where(x => x.Name == name).FirstOrDefault();
        }

        internal void RemoveCookie(string uri, string name)
        {
            var cookie = _cookieContainer.GetCookies(new Uri(uri)).Cast<Cookie>().Where(x=>x.Name == name).FirstOrDefault();
            if (cookie != null)
            {
                cookie.Expired = true;
            }
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
