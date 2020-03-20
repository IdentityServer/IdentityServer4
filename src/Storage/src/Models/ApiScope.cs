// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using System;
using System.Collections.Generic;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models access to an API scope
    /// </summary>
    public class ApiScope : Resource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiScope"/> class.
        /// </summary>
        public ApiScope()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiScope"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ApiScope(string name)
            : this(name, name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiScope"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        public ApiScope(string name, string displayName)
            : this(name, displayName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiScope"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="claimTypes">The user-claim types.</param>
        public ApiScope(string name, IEnumerable<string> claimTypes)
            : this(name, name, claimTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiScope"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="claimTypes">The user-claim types.</param>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public ApiScope(string name, string displayName, IEnumerable<string> claimTypes)
        {
            if (name.IsMissing()) throw new ArgumentNullException(nameof(name));

            Name = name;
            DisplayName = displayName;

            if (!claimTypes.IsNullOrEmpty())
            {
                foreach (var type in claimTypes)
                {
                    UserClaims.Add(type);
                }
            }
        }

        /// <summary>
        /// Specifies whether the user can de-select the scope on the consent screen. Defaults to false.
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to false.
        /// </summary>
        public bool Emphasize { get; set; } = false;
    }
}
