namespace Microsoft.Extensions.DependencyInjection
{
    public interface IIdentityServerBuilder
    {
        IServiceCollection Services { get; }
    }
}