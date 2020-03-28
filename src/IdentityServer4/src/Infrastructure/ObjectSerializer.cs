﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Specialized;
using System.Text.Json;
using IdentityServer4.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace IdentityServer4
{
    internal static class ObjectSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        static ObjectSerializer()
        {
            Settings.Converters.Add(new NameValueCollectionConverter());
        }

        public static string ToString(object o)
        {
            if (o is JObject) throw new Exception("fix JObject serialization");
            if (o is NameValueCollection) throw new Exception("fix NameValueCollection serialization");

            var options = new JsonSerializerOptions {IgnoreNullValues = true};

            return System.Text.Json.JsonSerializer.Serialize(o, options);

            //return JsonConvert.SerializeObject(o, Settings);
        }

        public static T FromString<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, Settings);
        }

        public static JObject ToJObject(object o)
        {
            return JObject.FromObject(o, Serializer);
        }
    }
}