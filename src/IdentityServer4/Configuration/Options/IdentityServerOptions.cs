// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer4.Core.Configuration
{
    /// <summary>
    /// The IdentityServerOptions class is the top level container for all configuration settings of IdentityServer.
    /// </summary>
    public class IdentityServerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerOptions"/> class with default values.
        /// </summary>
        public IdentityServerOptions()
        {
            SiteName = Constants.IdentityServerName;

            ProtocolLogoutUrls = new List<string>();
            RequireSsl = true;
            Endpoints = new EndpointOptions();
            AuthenticationOptions = new AuthenticationOptions();
            CspOptions = new CspOptions();
            EventsOptions = new EventsOptions();
            EnableWelcomePage = true;
            InputLengthRestrictions = new InputLengthRestrictions();
            DiscoveryOptions = new DiscoveryOptions();
        }

        /// <summary>
        /// Gets or sets the display name of the site used in standard views.
        /// </summary>
        /// <value>
        /// Display name of the site used in standard views.
        /// </value>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the unique name of this server instance, e.g. https://myissuer.com
        /// </summary>
        /// <value>
        /// Unique name of this server instance, e.g. https://myissuer.com
        /// </value>
        public string IssuerUri { get; set; }

        /// <summary>
        /// Gets or sets the X.509 certificate (and corresponding private key) for signing security tokens.
        /// </summary>
        /// <value>
        /// The signing certificate.
        /// </value>
        public X509Certificate2 SigningCertificate { get; set; }

        /// <summary>
        /// Gets or sets a secondary certificate that will appear in the discovery document. Can be used to prepare clients for certificate rollover
        /// </summary>
        /// <value>
        /// The secondary signing certificate.
        /// </value>
        public X509Certificate2 SecondarySigningCertificate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is required for IdentityServer. Defaults to `true`.
        /// </summary>
        /// <value>
        ///   <c>true</c> if SSL is required; otherwise, <c>false</c>.
        /// </value>
        public bool RequireSsl { get; set; }

        /// <summary>
        /// Gets or sets the endpoint configuration.
        /// </summary>
        /// <value>
        /// The endpoints configuration.
        /// </value>
        public EndpointOptions Endpoints { get; set; }

        /// <summary>
        /// Gets or sets the discovery endpoint configuration.
        /// </summary>
        /// <value>
        /// The discovery endpoint configuration.
        /// </value>
        public DiscoveryOptions DiscoveryOptions { get; set; }

        /// <summary>
        /// Gets or sets the authentication options.
        /// </summary>
        /// <value>
        /// The authentication options.
        /// </value>
        public AuthenticationOptions AuthenticationOptions { get; set; }

        /// <summary>
        /// Gets or sets the protocol logout urls.
        /// </summary>
        /// <value>
        /// The protocol logout urls.
        /// </value>
        public List<string> ProtocolLogoutUrls { get; set; }

        /// <summary>
        /// Gets or sets the CSP options.
        /// </summary>
        /// <value>
        /// The CSP options.
        /// </value>
        public CspOptions CspOptions { get; set; }

        /// <summary>
        /// Gets or sets the events options.
        /// </summary>
        /// <value>
        /// The events options.
        /// </value>
        public EventsOptions EventsOptions { get; set; }

        /// <summary>
        /// Gets or sets the max input length restrictions.
        /// </summary>
        /// <value>
        /// The length restrictions.
        /// </value>
        public InputLengthRestrictions InputLengthRestrictions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the welcome page is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the welcome page is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableWelcomePage { get; set; }
        
        internal IEnumerable<X509Certificate2> PublicKeysForMetadata
        {
            get
            {
                var keys = new List<X509Certificate2>();
                
                if (SigningCertificate != null)
                {
                    keys.Add(SigningCertificate);
                }

                if (SecondarySigningCertificate != null)
                {
                    keys.Add(SecondarySigningCertificate);
                }

                return keys;
            }
        }
    }
}