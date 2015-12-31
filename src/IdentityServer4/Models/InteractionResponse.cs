// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Models
{
    class InteractionResponse
    {
        public bool IsLogin { get; set; }
        public bool IsConsent { get; set; }
        public bool IsError { get { return Error != null; } }
        public AuthorizeError Error { get; set; }
    }
}