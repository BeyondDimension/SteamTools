using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlatformService(this IServiceCollection services, bool hasSteam, bool hasGUI, bool hasNotifyIcon)
        {
            if (OperatingSystem2.IsLinux)
            {
                services.AddSingleton<IHttpPlatformHelperService, LinuxClientHttpPlatformHelperServiceImpl>();
                services.AddSingleton<LinuxPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<LinuxPlatformServiceImpl>());
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}