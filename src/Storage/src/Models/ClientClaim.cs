// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Security.Claims;

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

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        public ClientClaim(string type, string value, string valueType = ClaimValueTypes.String)
        {
            Type = type;
            Value = value;
            ValueType = valueType;
        }
    }
}