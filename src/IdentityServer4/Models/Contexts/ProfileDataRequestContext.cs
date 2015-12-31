// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer4.Core.Models
{
    /// <summary>
    /// Class describing the profile data request
    /// </summary>
    public class ProfileDataRequestContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileDataRequestContext"/> class.
        /// </summary>
        public ProfileDataRequestContext()
        {
            IssuedClaims = Enumerable.Empty<Claim>();
        }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public ClaimsPrincipal Subject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all claims are requested.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all claims are requested; otherwise, <c>false</c>.
        /// </value>
        public bool AllClaimsRequested { get; set; }

        /// <summary>
        /// Gets or sets the requested claim types.
        /// </summary>
        /// <value>
        /// The requested claim types.
        /// </value>
        public IEnumerable<string> RequestedClaimTypes { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        /// <value>
        /// The client id.
        /// </value>
        public Client Client { get; set; }

        /// <summary>
        /// Gets or sets the caller.
        /// </summary>
        /// <value>
        /// The caller.
        /// </value>
        public string Caller { get; set; }

        /// <summary>
        /// Gets or sets the issued claims.
        /// </summary>
        /// <value>
        /// The issued claims.
        /// </value>
        public IEnumerable<Claim> IssuedClaims { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileDataRequestContext" /> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="client">The client.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="requestedClaimTypes">The requested claim types.</param>
        public ProfileDataRequestContext(ClaimsPrincipal subject, Client client, string caller, IEnumerable<string> requestedClaimTypes = null)
        {
            Subject = subject;
            Client = client;
            Caller = caller;

            if (requestedClaimTypes == null)
            {
                AllClaimsRequested = true;
            }
            else
            {
                RequestedClaimTypes = requestedClaimTypes;
            }

            IssuedClaims = Enumerable.Empty<Claim>();
        }
    }
}