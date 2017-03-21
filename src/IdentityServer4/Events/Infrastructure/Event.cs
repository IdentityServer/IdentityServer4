// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
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
        public Event(string category, string name, EventTypes type, int id, string message = null)
        {
            if (category.IsMissing()) throw new ArgumentNullException(nameof(category));

            Category = category;
            Name = name;
            EventType = type;
            Id = id;
            Message = message;
        }

        protected internal virtual Task PrepareAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets or sets the event category. <see cref="EventConstants.Categories"/> for a list of the defined categories.
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
        /// Gets or sets the event identifier. <see cref="EventConstants.Ids"/> for the list of the defined identifiers.
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

        protected static string ObfuscateToken(string token)
        {
            string last4chars = "****";
            if (token.IsPresent() && token.Length > 4)
            {
                last4chars = token.Substring(token.Length - 4);
            }

            return "****" + last4chars;
        }
    }
}