// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Models a client secret with identifier and expiration
    /// </summary>
    public class Secret
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>
        /// The expiration.
        /// </value>
        public DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// Gets or sets the type of the client secret.
        /// </summary>
        /// <value>
        /// The type of the client secret.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Secret"/> class.
        /// </summary>
        public Secret()
        {
            Type = Constants.SecretTypes.SharedSecret;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Secret"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expiration">The expiration.</param>
        public Secret(string value, DateTimeOffset? expiration = null)
            : this()
        {
            Value = value;
            Expiration = expiration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Secret" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        /// <param name="expiration">The expiration.</param>
        public Secret(string value, string description, DateTimeOffset? expiration = null)
            : this()
        {
            Description = description;
            Value = value;
            Expiration = expiration;
        }
    }
}