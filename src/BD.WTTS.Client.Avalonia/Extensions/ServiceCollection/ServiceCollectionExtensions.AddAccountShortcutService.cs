namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
#if WINDOWS || MACOS || LINUX
    public static IServiceCollection AddAccountShortcutService(this IServiceCollection services)
    {
        services.AddSingleton<IAccountShortcutService, AccountShortcutServiceImpl>();
        return services;
    }
#endif
}
