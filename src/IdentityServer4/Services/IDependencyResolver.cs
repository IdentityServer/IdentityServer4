// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Services
{
    /// <summary>
    /// Allows resolving dependencies from the dependency injection system.
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Resolves the dependency based upon the type. If name is provided then the dependency is also resolved by name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns>The dependency.</returns>
        T Resolve<T>(string name = null);
    }
}
