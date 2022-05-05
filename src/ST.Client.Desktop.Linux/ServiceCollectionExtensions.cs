using System;
using System.Application;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlatformService(this IServiceCollection services, StartupOptions options)
        {
            if (OperatingSystem2.IsLinux)
            {
                services.AddSingleton<IHttpPlatformHelperService, LinuxClientHttpPlatformHelperServiceImpl>();
                services.AddSingleton<LinuxPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<LinuxPlatformServiceImpl>());
                if (options.HasSteam)
                {
                    services.AddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
                }
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}