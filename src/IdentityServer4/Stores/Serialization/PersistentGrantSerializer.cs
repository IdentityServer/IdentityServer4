// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Services;
using Newtonsoft.Json;

namespace IdentityServer4.Stores.Serialization
{
    public class PersistentGrantSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public PersistentGrantSerializer(IClientStore clientStore, IScopeStore scopeStore)
        {
            _settings = new JsonSerializerSettings();
            _settings.ContractResolver = new CustomContractResolver();
            _settings.Converters.Add(new ClaimConverter());
            _settings.Converters.Add(new ClaimsPrincipalConverter());
            _settings.Converters.Add(new ClientConverter(clientStore));
            _settings.Converters.Add(new ScopeConverter(scopeStore));
        }

        public virtual string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }

        public virtual T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}