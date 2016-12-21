// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Quickstart.UI.Models;

namespace IdentityServer4.Quickstart.UI.Services
{
    public class ProcessConsentResult
    {
        public string RedirectUri { get; set; }
        public ConsentViewModel ViewModel { get; set; }
        public string Error { get; set; }
    }
}
