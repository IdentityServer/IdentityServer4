using Microsoft.Extensions.DependencyInjection;
using System;

namespace IdentityServer4.Core.Configuration
{
    public class IdentityServerBuilder : IIdentityServerBuilder
    {
        public IdentityServerBuilder(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}