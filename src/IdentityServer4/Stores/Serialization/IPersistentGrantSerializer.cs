// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Stores.Serialization
{
    public interface IPersistentGrantSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }
}