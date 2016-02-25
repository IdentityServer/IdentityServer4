using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    interface IEndSessionResponseGenerator
    {
        Task<Message<SignOutRequest>> ProcessAsync(EndSessionRequestValidationResult validationResult);
    }
}
