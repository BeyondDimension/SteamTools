using Microsoft.Extensions.DependencyInjection;
using System.Application.Models;
using System.Application.Services.Implementation;
using System.Application.UI.ViewModels;

namespace System.Application.UI
{
    internal static class Startup
    {
        public static void Init()
        {
            FileSystemDesktop.InitFileSystem();
            ModelValidatorProvider.Init();
            DI.Init(ConfigureServices);
        }

        static void ConfigureServices(IServiceCollection services)
        {
            // 添加日志实现
            services.AddDesktopLogging();

            // 模型验证框架
            services.TryAddModelValidator();

            //var options = AppClientAttribute.Get<AppSettings>();
            var options = new AppSettings
            {
                //AppSecretVisualStudioAppCenter = "",
            };
            // app 配置项
            services.TryAddOptions(options);

            // 键值对存储
            services.TryAddStorage();

            // 业务平台用户管理
            services.TryAddUserManager();

            // 服务端API调用
            services.TryAddCloudServiceClient<CloudServiceClient>();

            // 桌面平台服务 此项放在其他通用业务实现服务之前
            services.AddDesktopPlatformService();

            // 主线程助手类(MainThreadDesktop)
            services.AddMainThreadPlatformService();

            // 业务用户配置文件服务
            services.AddConfigFileService();

            // hosts 文件助手服务
            services.AddHostsFileService();

            // 通用 Http 服务
            services.AddHttpService();

            // Steam 相关助手、工具类服务
            services.AddSteamService();

            // Steamworks LocalApi Service
            services.TryAddSteamworksLocalApiService();

            // SteamDb WebApi Service
            services.AddSteamDbWebApiService();

            // Steamworks WebApi Service
            services.AddSteamworksWebApiService();

            // 应用程序更新服务
            services.AddAppUpdateService();
        }
    }
}