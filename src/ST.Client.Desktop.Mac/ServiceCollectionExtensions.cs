using System;
using System.Application;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlatformService(this IServiceCollection services, StartupOptions options)
        {
            if (OperatingSystem2.IsMacOS)
            {
                services.AddSingleton<IHttpPlatformHelperService, MacClientHttpPlatformHelperServiceImpl>();
                services.AddSingleton(AppDelegate.Instance!);
                services.AddSingleton<MacPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<MacPlatformServiceImpl>());
                services.AddSingleton<IEmailPlatformService>(s => s.GetRequiredService<MacPlatformServiceImpl>());
                //services.AddPlatformNotificationService();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }

        ///// <summary>
        ///// 添加适用于 macOS 的 <see cref="INotificationService"/>
        ///// </summary>
        ///// <param name="services"></param>
        ///// <returns></returns>
        //static IServiceCollection AddPlatformNotificationService(this IServiceCollection services)
        //{
        //    services.AddSingleton<INotificationService, MacNotificationServiceImpl>();
        //    return services;
        //}
    }
}