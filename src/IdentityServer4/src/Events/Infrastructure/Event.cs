// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.Logging;
using System;
using System.Threading.Tasks;

namespace IdentityServer4.Events
{
    /// <summary>
    /// Models base class for events raised from IdentityServer.
    /// </summary>
    public abstract class Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Event" /> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException">category</exception>
        protected Event(string category, string name, EventTypes type, int id, string message = null)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Name = name ?? throw new ArgumentNullException(nameof(name));

            EventType = type;
            Id = id;
            Message = message;
        }

        /// <summary>
        /// Allows implementing custom initialization logic.
        /// </summary>
        /// <returns></returns>
        protected internal virtual Task PrepareAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [Newtonsoft.Json.JsonProperty(Order = -99)]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Newtonsoft.Json.JsonProperty(Order = -100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        [Newtonsoft.Json.JsonProperty(Order = -98)]
        public EventTypes EventType { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Newtonsoft.Json.JsonProperty(Order = -97)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the event message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the per-request activity identifier.
        /// </summary>
        /// <value>
        /// The activity identifier.
        /// </value>
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the time stamp when the event was raised.
        /// </summary>
        /// <value>
        /// The time stamp.
        /// </value>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the server process identifier.
        /// </summary>
        /// <value>
        /// The process identifier.
        /// </value>
        public int ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the local ip address of the current request.
        /// </summary>
        /// <value>
        /// The local ip address.
        /// </value>
        public string LocalIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the remote ip address of the current request.
        /// </summary>
        /// <value>
        /// The remote ip address.
        /// </value>
        public string RemoteIpAddress { get; set; }

        /// <summary>
        /// Obfuscates a token.
        /// </summary>
        /// <param name="value">The token.</param>
        /// <returns></returns>
        protected static string Obfuscate(string value)
        {
            var last4Chars = "****";
            if (value.IsPresent() && value.Length > 4)
            {
                last4Chars = value.Substring(value.Length - 4);
            }

            return "****" + last4Chars;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return LogSerializer.Serialize(this);
        }
    }
}