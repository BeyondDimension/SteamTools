// ReSharper disable once CheckNamespace
using BD.WTTS.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 <see cref="IViewModelManager"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddViewModelManager(this IServiceCollection services)
    {
        services.AddTransient<CommunityProxyPageViewModel>();
        services.AddTransient<ProxyScriptManagePageViewModel>();
        services.AddTransient<SteamAccountPageViewModel>();
        services.AddTransient<GameListPageViewModel>();
        services.AddTransient<LocalAuthPageViewModel>();
        services.AddTransient<ArchiSteamFarmPlusPageViewModel>();
        services.AddTransient<GameRelatedPageViewModel>();
        services.AddTransient<GameRelatedPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();
        services.AddTransient<AboutPageViewModel>();
        services.AddTransient<StartPageViewModel>();

#if DEBUG
        services.AddTransient<DebugPageViewModel>();
#endif

        services.AddSingleton<IViewModelManager, ViewModelManager>();
        return services;
    }
}