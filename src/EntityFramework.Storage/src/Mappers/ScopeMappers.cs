// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AutoMapper;
using IdentityServer4.EntityFramework.Entities;

namespace IdentityServer4.EntityFramework.Mappers
{
    /// <summary>
    /// Extension methods to map to/from entity/model for scopes.
    /// </summary>
    public static class ScopeMappers
    {
        static ScopeMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ScopeMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Models.Scope ToModel(this Scope entity)
        {
            return entity == null ? null : Mapper.Map<Models.Scope>(entity);
        }

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static Scope ToEntity(this Models.Scope model)
        {
            return model == null ? null : Mapper.Map<Scope>(model);
        }
    }
}