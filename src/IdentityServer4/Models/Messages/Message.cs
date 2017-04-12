// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Base class for data that needs to be written out as cookies.
    /// </summary>
    public class Message<TModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message{TModel}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Message(TModel data)
        {
            Created = IdentityServerDateTime.UtcNow.Ticks;
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

    /// <summary>
    /// Message with Id
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <seealso cref="IdentityServer4.Models.Message{TModel}" />
    public class MessageWithId<TModel> : Message<TModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageWithId{TModel}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public MessageWithId(TModel data) : base(data)
        {
            Id = CryptoRandom.CreateUniqueId(16);
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }
    }
}