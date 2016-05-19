// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net.Http;

namespace IdentityServer4.Tests.Common
{
    public class BrowserClient : HttpClient
    {
        public BrowserClient(BrowserHandler browserHandler)
            : base(browserHandler)
        {
            BrowserHandler = browserHandler;
        }

        public BrowserHandler BrowserHandler { get; private set; }

        public bool AllowCookies
        {
            get { return BrowserHandler.AllowCookies; }
            set { BrowserHandler.AllowCookies = value; }
        }
        public bool AllowAutoRedirect
        {
            get { return BrowserHandler.AllowAutoRedirect; }
            set { BrowserHandler.AllowAutoRedirect = value; }
        }
        public int ErrorRedirectLimit
        {
            get { return BrowserHandler.ErrorRedirectLimit; }
            set { BrowserHandler.ErrorRedirectLimit = value; }
        }
        public int StopRedirectingAfter
        {
            get { return BrowserHandler.StopRedirectingAfter; }
            set { BrowserHandler.StopRedirectingAfter = value; }
        }
    }
}
