// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Models
{
    class AuthorizeError
    {
        public string Error { get; set; }
        public string ResponseMode { get; set; }
        public string ErrorUri { get; set; }
        public string State { get; set; }
    }
}
