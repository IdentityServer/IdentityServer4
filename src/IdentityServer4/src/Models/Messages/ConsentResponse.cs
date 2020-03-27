﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models the user's response to the consent screen.
    /// </summary>
    public class ConsentResponse
    {
        /// <summary>
        /// A denied consent response
        /// </summary>
        public static ConsentResponse Denied = new ConsentResponse();

        /// <summary>
        /// Gets if consent was granted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent was granted; otherwise, <c>false</c>.
        /// </value>
        public bool Granted => ScopesValuesConsented != null && ScopesValuesConsented.Any();

        /// <summary>
        /// Gets or sets the scope values consented to.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public IEnumerable<string> ScopesValuesConsented { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wishes the consent to be remembered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if consent is to be remembered; otherwise, <c>false</c>.
        /// </value>
        public bool RememberConsent { get; set; }

        /// <summary>
        /// Gets the description of the device.
        /// </summary>
        /// <value>
        /// The description of the device.
        /// </value>
        public string Description { get; set; }
    }
}
