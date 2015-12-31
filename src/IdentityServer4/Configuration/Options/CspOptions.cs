// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Configuration
{
    /// <summary>
    /// Configures Content Security Policy (CSP) for HTML pages rendered by IdentityServer.
    /// </summary>
    public class CspOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CspOptions"/> class.
        /// </summary>
        public CspOptions()
        {
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether CSP is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Allows additional script sources to be indicated.
        /// </summary>
        /// <value>
        /// The script source.
        /// </value>
        public string ScriptSrc { get; set; }

        /// <summary>
        /// Allows additional style sources to be indicated.
        /// </summary>
        /// <value>
        /// The style source.
        /// </value>
        public string StyleSrc { get; set; }

        /// <summary>
        /// Allows additional font sources to be indicated.
        /// </summary>
        /// <value>
        /// The font source.
        /// </value>
        public string FontSrc { get; set; }

        /// <summary>
        /// Allows additional connect sources to be indicated.
        /// </summary>
        /// <value>
        /// The connect source.
        /// </value>
        public string ConnectSrc { get; set; }

        /// <summary>
        /// Allows additional image sources to be indicated.
        /// </summary>
        /// <value>
        /// The connect source.
        /// </value>
        public string ImgSrc { get; set; }
    }
}
