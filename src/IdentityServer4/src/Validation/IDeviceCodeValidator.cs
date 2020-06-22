// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// The device code validator
    /// </summary>
    public interface IDeviceCodeValidator
    {
        /// <summary>
        /// Validates the device code.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns></returns>
        Task ValidateAsync(DeviceCodeValidationContext context, CancellationToken cancellationToken = default);
    }
}