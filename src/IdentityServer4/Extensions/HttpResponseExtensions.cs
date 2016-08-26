﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpResponseExtensions
    {
        public static async Task WriteJsonAsync(this HttpResponse response, object o)
        {
            var json = ObjectSerializer.ToString(o);
            await response.WriteJsonAsync(json);
        }

        public static async Task WriteJsonAsync(this HttpResponse response, string json)
        {
            response.ContentType = "application/json";
            await response.WriteAsync(json);
        }

        public static void SetNoCache(this HttpResponse response)
        {
            response.Headers.Add("Cache-Control", "no-store, no-cache, max-age=0");
            response.Headers.Add("Pragma", "no-cache");
        }
    }
}