// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer.UnitTests.Common
{
    public class MockDeviceCodeStore : IDeviceCodeStore
    {
        public Dictionary<string, DeviceCode> Codes { get; set; } = new Dictionary<string, DeviceCode>();

        public Task<string> StoreDeviceCodeAsync(DeviceCode code)
        {
            var id = Guid.NewGuid().ToString();
            Codes[id] = code;
            return Task.FromResult(id);
        }

        public Task<DeviceCode> GetDeviceCodeAsync(string code)
        {
            DeviceCode val = null;
            if (code != null)
            {
                Codes.TryGetValue(code, out val);
            }
            return Task.FromResult(val);
        }

        public Task RemoveDeviceCodeAsync(string code)
        {
            if (code != null && Codes.ContainsKey(code))
            {
                Codes.Remove(code);
            }
            return Task.CompletedTask;
        }
    }
}