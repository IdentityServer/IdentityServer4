﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.



using System;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Base class for data that needs to be written out as cookies.
    /// </summary>
    public class Message<TModel>
    {
        /// <summary>
        /// Should only be used from unit tests
        /// </summary>
        /// <param name="data"></param>
        internal Message(TModel data) : this(data, DateTime.UtcNow)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Message{TModel}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="now">The current UTC date/time.</param>
        public Message(TModel data, DateTime now)
        {
            Created = now.Ticks;
            Data = data;
        }

        /// <summary>
        /// Gets or sets the UTC ticks the <see cref="Message{TModel}" /> was created.
        /// </summary>
        /// <value>
        /// The created UTC ticks.
        /// </value>
        public long Created { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public TModel Data { get; set; }
    }
}