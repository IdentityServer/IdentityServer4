// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Models
{
    public class DeviceFlowInteractionResult
    {
        public string ErrorDescription { get; set; }
        public bool IsError { get; private set; }

        public bool IsAccessDenied { get; set; }

        public static DeviceFlowInteractionResult Failure(string errorDescription = null)
        {
            return new DeviceFlowInteractionResult
            {
                IsError = true,
            };
        }
    }
}