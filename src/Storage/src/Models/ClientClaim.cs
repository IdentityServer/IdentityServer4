// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.Models
{
    /// <summary>
    /// A client claim
    /// </summary>
    public class ClientClaim
    {
        /// <summary>
        /// The claim type
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// The claim value
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// The claim value type
        /// </summary>
        public string ValueType { get; set; }
    }
}