// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Extensions;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Base class for data that needs to be written out as cookies.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        internal Message()
        {
            Created = DateTimeOffsetHelper.UtcNow.Ticks;
        }

        /// <summary>
        /// Gets or sets the UTC ticks the <see cref="Message"/> was created.
        /// </summary>
        /// <value>
        /// The created UTC ticks.
        /// </value>
        public long Created { get; set; }
    }
}