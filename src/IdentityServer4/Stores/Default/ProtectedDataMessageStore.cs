﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using IdentityModel;
using System.Text;
using System;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Stores
{
    internal class ProtectedDataMessageStore<TModel> : IMessageStore<TModel>
    {
        const string purpose = "IdentityServer4.Stores.ProtectedDataMessageStore";

        private readonly IDataProtector _protector;
        private readonly ILogger _logger;

        public ProtectedDataMessageStore(IDataProtectionProvider provider, ILogger<ProtectedDataMessageStore<TModel>> logger)
        {
            _protector = provider.CreateProtector(purpose);
            _logger = logger;
        }

        public Task DeleteAsync(string id)
        {
            return Task.FromResult(0);
        }

        public Task<Message<TModel>> ReadAsync(string value)
        {
            Message<TModel> result = null;

            if (!String.IsNullOrWhiteSpace(value))
            {
                try
                {
                    var bytes = Base64Url.Decode(value);
                    bytes = _protector.Unprotect(bytes);
                    var json = Encoding.UTF8.GetString(bytes);
                    result = ObjectSerializer.FromString<Message<TModel>>(json);
                }
                catch(Exception ex)
                {
                    _logger.LogError("Exception reading message: {0}", ex.Message);
                }
            }

            return Task.FromResult(result);
        }

        public Task<string> WriteAsync(Message<TModel> message)
        {
            string value = null;

            try
            {
                var json = ObjectSerializer.ToString(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                bytes = _protector.Protect(bytes);
                value = Base64Url.Encode(bytes);
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception writing message: {0}", ex.Message);
            }

            return Task.FromResult(value);
        }

        public Task WriteAsync(string id, Message<TModel> message)
        {
            throw new NotImplementedException();
        }
    }
}
