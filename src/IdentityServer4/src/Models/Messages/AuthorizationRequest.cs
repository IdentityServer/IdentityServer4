﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Validation;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Represents contextual information about a authorization request.
    /// </summary>
    public class AuthorizationRequest
    {
        /// <summary>
        /// The client.
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// The display mode passed from the authorization request.
        /// </summary>
        /// <value>
        /// The display mode.
        /// </value>
        public string DisplayMode { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public string RedirectUri { get; set; }

        /// <summary>
        /// The UI locales passed from the authorization request.
        /// </summary>
        /// <value>
        /// The UI locales.
        /// </value>
        public string UiLocales { get; set; }

        /// <summary>
        /// The external identity provider requested. This is used to bypass home realm 
        /// discovery (HRD). This is provided via the <c>"idp:"</c> prefix to the <c>acr</c> 
        /// parameter on the authorize request.
        /// </summary>
        /// <value>
        /// The external identity provider identifier.
        /// </value>
        public string IdP { get; set; }

        /// <summary>
        /// The tenant requested. This is provided via the <c>"tenant:"</c> prefix to 
        /// the <c>acr</c> parameter on the authorize request.
        /// </summary>
        /// <value>
        /// The tenant.
        /// </value>
        public string Tenant { get; set; }

        /// <summary>
        /// The expected username the user will use to login. This is requested from the client 
        /// via the <c>login_hint</c> parameter on the authorize request.
        /// </summary>
        /// <value>
        /// The LoginHint.
        /// </value>
        public string LoginHint { get; set; }

        /// <summary>
        /// Gets or sets the collection of prompt modes.
        /// </summary>
        /// <value>
        /// The collection of prompt modes.
        /// </value>
        public IEnumerable<string> PromptModes { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// The acr values passed from the authorization request.
        /// </summary>
        /// <value>
        /// The acr values.
        /// </value>
        public IEnumerable<string> AcrValues { get; set; }

        /// <summary>
        /// The validated resources.
        /// </summary>
        public ResourceValidationResult ValidatedResources { get; set; }

        /// <summary>
        /// Gets the entire parameter collection.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public NameValueCollection Parameters { get; }

        /// <summary>
        /// Gets the validated contents of the request object (if present)
        /// </summary>
        /// <value>
        /// The request object values
        /// </value>
        public Dictionary<string, string> RequestObjectValues { get; } = new Dictionary<string, string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRequest"/> class.
        /// </summary>
        public AuthorizationRequest()
        {
            // public for testing
            Parameters = new NameValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRequest"/> class.
        /// </summary>
        internal AuthorizationRequest(ValidatedAuthorizeRequest request)
        {
            Client = request.Client;
            RedirectUri = request.RedirectUri;
            DisplayMode = request.DisplayMode;
            UiLocales = request.UiLocales;
            IdP = request.GetIdP();
            Tenant = request.GetTenant();
            LoginHint = request.LoginHint;
            PromptModes = request.PromptModes;
            AcrValues = request.GetAcrValues();
            ValidatedResources = request.ValidatedResources;
            Parameters = request.Raw;
            RequestObjectValues = request.RequestObjectValues;
        }
    }
}