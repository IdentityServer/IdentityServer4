// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models the request from a client to sign the user out.
    /// </summary>
    public class SignOutResponse
    {
        public SignOutResponse()
        {
            RemoveAuthenticationCookie = true;
        }

        public bool RemoveAuthenticationCookie { get; set; }
    }
}
