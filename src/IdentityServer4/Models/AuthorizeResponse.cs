// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Validation;

namespace IdentityServer4.Core.Models
{
    class AuthorizeResponse
    {
        public ValidatedAuthorizeRequest Request { get; set; }
        public string RedirectUri { get; set; }
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public int AccessTokenLifetime { get; set; }
        public string Code { get; set; }
        public string State { get; set; }
        public string Scope { get; set; }
        public string SessionState { get; set; }

        public string Error { get; set; }
        public bool IsError { get; set; }
    }
}