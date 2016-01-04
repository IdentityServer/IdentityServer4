// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Represents contextual information about a login request.
    /// </summary>
    public class SignInResponse 
    {
        public SignInResponse()
        {
        }

        //public SignInResponse(string subject)
        //{
        //    Subject = subject;
        //}

        //public string Subject { get; set; }
    }
}
