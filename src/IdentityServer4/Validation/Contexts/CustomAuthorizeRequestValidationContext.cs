using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.Validation
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
