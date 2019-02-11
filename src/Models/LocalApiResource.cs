// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace IdentityServer4.Models
{

    /// <summary>
    /// Models a local API
    /// </summary>
    /// <seealso cref="IdentityServer4.Models.ApiResource" />
    public class LocalApiResource : ApiResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalApiResource"/> class.
        /// </summary>
        public LocalApiResource()
        {
            Name = IdentityServerConstants.LocalApiName;
            DisplayName = "IdentityServer API";
            Description = "IdentityServer API";

            Scopes = new List<Scope>
            {
                new Scope
                {
                    Name = IdentityServerConstants.LocalApiName,
                    ShowInDiscoveryDocument = false
                }
            };
        }
    }
}