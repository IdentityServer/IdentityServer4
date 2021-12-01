﻿using Bornlogic.IdentityServer.Validation.Models;

namespace Bornlogic.IdentityServer.Validation.Contexts
{
    /// <summary>
    /// Context for custom authorize request validation.
    /// </summary>
    public class CustomAuthorizeRequestValidationContext
    {
        /// <summary>
        /// The result of custom validation. 
        /// </summary>
        public AuthorizeRequestValidationResult Result { get; set; }
    }
}
