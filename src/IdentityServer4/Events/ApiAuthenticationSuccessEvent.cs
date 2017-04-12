﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Events
{
    /// <summary>
    /// Event for successful API authentication
    /// </summary>
    /// <seealso cref="IdentityServer4.Events.Event" />
    public class ApiAuthenticationSuccessEvent : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiAuthenticationSuccessEvent"/> class.
        /// </summary>
        /// <param name="apiName">Name of the API.</param>
        /// <param name="authenticationMethod">The authentication method.</param>
        public ApiAuthenticationSuccessEvent(string apiName, string authenticationMethod)
            : base(EventCategories.Authentication, 
                  "API Authentication Success",
                  EventTypes.Success, 
                  EventIds.ApiAuthenticationSuccess)
        {
            ApiName = apiName;
            AuthenticationMethod = authenticationMethod;
        }

        /// <summary>
        /// Gets or sets the name of the API.
        /// </summary>
        /// <value>
        /// The name of the API.
        /// </value>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the authentication method.
        /// </summary>
        /// <value>
        /// The authentication method.
        /// </value>
        public string AuthenticationMethod { get; set; }
    }
}
