// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models the message returned from the consent page.
    /// </summary>
    public class UserConsentResponseMessage : Message
    {
        public UserConsentResponseMessage()
        {
            Consent = new UserConsent();
            AuthorizeRequestParameters = new NameValueCollection();
        }

        public UserConsent Consent { get; set; }
        public NameValueCollection AuthorizeRequestParameters { get; set; }
    }
}
