using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Results;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    interface IAuthorizeEndpointResultGenerator
    {
        Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request);
        Task<IEndpointResult> CreateLoginResultAsync(SignInMessage message);
        Task<IEndpointResult> CreateConsentResultAsync();
        Task<IEndpointResult> CreateAuthorizeResultAsync(AuthorizeResponse response);
    }
}
