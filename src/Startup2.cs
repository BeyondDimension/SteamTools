#if !__MOBILE__ && !CONSOLEAPP
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
#else
using Xamarin.Essentials;
#endif
#if UI_DEMO
using Moq;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Application.Models;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Application.Services.Implementation;
using System.Application.UI;
using System.Collections.Generic;
using System.Reflection;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Linq;
using static System.Application.Browser2;
using System.Properties;
using System.IO;
using static System.Application.AppClientAttribute;
using System.Net;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Settings;
#if __ANDROID__
using Xamarin.Android.Net;
using Program = System.Application.UI.MainApplication;
using PlatformApplication = System.Application.UI.MainApplication;
using System.Application.UI.Resx;
using System.Windows;
#elif __IOS__
using Program = System.Application.UI.AppDelegate;
#elif !__MOBILE__
using PlatformApplication = System.Application.UI.App;
#endif
#if StartupTrace
using System.Diagnostics;
#endif
using _ThisAssembly = System.Properties.ThisAssembly;

namespace System.Application
{
    static partial class Startup
    {
        static bool isInitialized;

        /// <summary>
        /// 初始化启动
        /// </summary>
        public static void Init(DILevel level)
        {
            if (!isInitialized)
            {
                isInitialized = true;
#if StartupTrace
                StartupTrace.Restart("Startup.InitFileSystem");
#endif
                if (level.HasFlag(DILevel.ServerApiClient))
                {
                    ModelValidatorProvider.Init();
#if StartupTrace
                    StartupTrace.Restart("ModelValidatorProvider.Init");
#endif
                }
                InitDI(level);
#if StartupTrace
                StartupTrace.Restart($"InitDI: {level}");
#endif
                static void InitDI(DILevel level)
                {
#if UI_DEMO
                    DI.Init(new MockServiceProvider(ConfigureDemoServices));
#else
                    DI.Init(s => ConfigureServices(s, level));
                    static void ConfigureServices(IServiceCollection services, DILevel level)
                    {
                        ConfigureRequiredServices(services);
#if StartupTrace
                        StartupTrace.Restart("DI.ConfigureRequiredServices");
#endif
                        ConfigureDemandServices(services, level);
#if StartupTrace
                        StartupTrace.Restart("DI.ConfigureDemandServices");
#endif
                    }
#endif
                }
            }
        }

        /// <summary>
        /// 配置任何进程都必要的依赖注入服务
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureRequiredServices(IServiceCollection services)
        {
            // 添加日志实现
            services.AddGeneralLogging();
#if __MOBILE__
            // 添加运行时权限
            services.TryAddPermissions();
            services.AddPlatformPermissions();
#endif
#if !CONSOLEAPP
            // 添加 app 配置项
            services.TryAddOptions(AppSettings);
            // 键值对存储
            services.TryAddStorage();

            // 添加安全服务
            services.AddSecurityService<EmbeddedAesDataProtectionProvider, LocalDataProtectionProvider>();
#endif
        }

        /// <summary>
        /// 配置按需使用的依赖注入服务
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureDemandServices(IServiceCollection services, DILevel level)
        {
            var hasMainProcessRequired = level.HasFlag(DILevel.MainProcessRequired);
#if !__MOBILE__
#if !CONSOLEAPP
            HasNotifyIcon = hasMainProcessRequired;
#endif
#endif
#if !CONSOLEAPP
            var hasGUI = level.HasFlag(DILevel.GUI);
            var hasServerApiClient = level.HasFlag(DILevel.ServerApiClient);
#endif
            var hasHttpClientFactory = level.HasFlag(DILevel.HttpClientFactory);
#if !CONSOLEAPP
            var hasHttpProxy = level.HasFlag(DILevel.HttpProxy);
#endif
            var hasHosts = level.HasFlag(DILevel.Hosts);
            var hasSteam = level.HasFlag(DILevel.Steam);
#if !UI_DEMO && !__MOBILE__
            // 桌面平台服务 此项放在其他通用业务实现服务之前
            services.AddPlatformService(hasSteam, hasGUI, HasNotifyIcon);
#endif
#if __MOBILE__
            services.AddMobilePlatformService(hasGUI);
#endif
#if StartupTrace
            StartupTrace.Restart("DI.ConfigureDemandServices.Calc");
#endif
#if !CONSOLEAPP
            if (hasGUI)
            {
                services.AddPinyin();
#if __MOBILE__
                services.TryAddFontManager();
#else
                services.TryAddAvaloniaFontManager(useGdiPlusFirst: true);
#endif
                // 添加 Toast 提示服务
#if !DEBUG
                services.AddStartupToastIntercept();
#endif
                services.TryAddToast();

                services.AddSingleton<IApplication>(_ => PlatformApplication.Instance);
#if __MOBILE__
                // 添加电话服务
                services.AddTelephonyService();

                services.AddMSALPublicClientApp(AppSettings.MASLClientId);
#else
                services.AddSingleton<IApplication>(_ => PlatformApplication.Instance);
                services.AddSingleton<IAvaloniaApplication>(_ => PlatformApplication.Instance);
                services.TryAddSingleton<IClipboardPlatformService>(_ => PlatformApplication.Instance);

                // 添加管理主窗口服务
                services.AddViewModelManager();

                // 添加主线程助手(MainThreadDesktop)
                services.AddMainThreadPlatformService();

                services.TryAddFilePickerPlatformService();
#endif
                #region MessageBox

                /* System.Windows.MessageBox 在 WPF 库中，仅支持 Win 平台
                 * 改为 System.Windows.MessageBoxCompat 可跨平台兼容
                 * 在其他平台上使用 MessageBox.Avalonia 库实现
                 * API变更说明：
                 * - 如果需要获取返回值，即点击那个按钮，则使用异步版本 ShowAsync
                 * - 如果不需要获取返回值，则可直接使用 同步版本 Show
                 * 注意事项：
                 * - 图标(Icon)与按钮(Button)不要使用标记为 Obsolete 的
                 * - WPF 中 显示窗口(Show)会锁死父窗口等，类似 ShowDialog
                 * - MessageBox.Avalonia 中则不会锁死窗口
                 * 已知问题：
                 * - 在 MessageBox.Avalonia 中
                 *  - 如果内容文本(messageBoxText)过短 UI 上的文字显示不全
                 *  - 点击窗口按 Ctrl+C 无法复制弹窗中的文本内容
                 *  - 按钮文本(ButtonText)缺少本地化翻译(Translate)
                 *  - 某些图标图片与枚举值不太匹配，例如 Information
                 */
                services.AddWindowManager();

#if WINDOWS
                // 可选项，在 Win 平台使用 WPF 实现的 MessageBox
                //services.AddSingleton<IMessageBoxCompatService, WPFMessageBoxCompatService>();
#endif

                #endregion

                services.TryAddBiometricService();
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.GUI");
#endif
            }
#endif
            if (hasHttpClientFactory
#if !CONSOLEAPP
                || hasServerApiClient
#endif
                )
            {
#if __MOBILE__
                // 添加 Http 平台助手移动端实现
                services.AddPlatformHttpPlatformHelper();
#else
                // 添加 Http 平台助手桌面端实现
                services.TryAddClientHttpPlatformHelperService();
#endif
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.HttpPlatformHelper");
#endif
            }

            if (hasHttpClientFactory)
            {
#if __MOBILE__
                // 添加 HttpClientFactory 平台原生实现
                services.AddNativeHttpClient();
#endif
                // 通用 Http 服务
                services.AddHttpService();
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.HttpClientFactory");
#endif
            }
#if !__MOBILE__
            services.TryAddScriptManager();
#endif
#if StartupTrace
            StartupTrace.Restart("DI.ConfigureDemandServices.ScriptManager");
#endif

#if !CONSOLEAPP && !__MOBILE__
            if (hasHttpProxy)
            {
                // 通用 Http 代理服务
                services.AddHttpProxyService();
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.HttpProxy");
#endif
            }
#endif
#if !CONSOLEAPP
            if (hasServerApiClient)
            {
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.AppSettings");
#endif
                // 添加模型验证框架
                services.TryAddModelValidator();

                // 添加服务端API调用
                services.TryAddCloudServiceClient<CloudServiceClient>(c =>
                {
#if NETCOREAPP3_0_OR_GREATER
                    c.DefaultRequestVersion = HttpVersion.Version20;
#endif
#if NET5_0_OR_GREATER
                    c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
#endif
                }, configureHandler:
#if NETCOREAPP2_1_OR_GREATER
                () => new SocketsHttpHandler
                {
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.GZip,
                }
#elif __ANDROID__
                () => new AndroidClientHandler
                {
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.GZip,
                }
#else
            null
#endif
            );

                services.AddAutoMapper();

                // 添加仓储服务
                services.AddRepositories();

                // 业务平台用户管理
                services.TryAddUserManager();
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.ServerApiClient");
#endif
            }
#endif
#if !CONSOLEAPP
            // 添加通知服务
            AddNotificationService();
#if StartupTrace
            StartupTrace.Restart("DI.ConfigureDemandServices.AddNotificationService");
#endif
            void AddNotificationService()
            {
#if !__MOBILE__
                if (!Program.IsMainProcess) return;
#endif
                services.TryAddNotificationService();
            }
#endif
#if !__MOBILE__
#if !CONSOLEAPP
            if (hasHosts)
            {
                // hosts 文件助手服务
                services.AddHostsFileService();
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.HostsFileService");
#endif
            }
#endif
            if (hasSteam)
            {
                // Steam 相关助手、工具类服务
                services.AddSteamService();

                // Steamworks LocalApi Service
                services.TryAddSteamworksLocalApiService();

                // SteamDb WebApi Service
                services.AddSteamDbWebApiService();

                // Steamworks WebApi Service
                services.AddSteamworksWebApiService();

                // ASF Service
                services.AddArchiSteamFarmService();
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.Steam");
#endif
            }
#endif
#if !CONSOLEAPP
            if (hasMainProcessRequired)
            {
                // 应用程序更新服务
                services.AddApplicationUpdateService();
#if StartupTrace
                StartupTrace.Restart("DI.ConfigureDemandServices.AppUpdateService");
#endif
            }
#endif
        }

#if UI_DEMO
        static void ConfigureDemoServices(IServiceCollection services)
        {
            services.AddLogging(cfg => cfg.AddProvider(NullLoggerProvider.Instance));

            services.AddSingleton<ICloudServiceClient, MockCloudServiceClient>();
        }
#endif

#if !CONSOLEAPP
        static AppSettings? mAppSettings;
        public static AppSettings AppSettings
        {
            get
            {
                if (mAppSettings == null)
                {
#if StartupTrace
                    var stopwatch = Stopwatch.StartNew();
#endif
                    mAppSettings = new AppSettings
                    {
                        AppVersion = GetResValueGuid("app-id", isSingle: false, ResValueFormat.StringGuidN),
                        AesSecret = GetResValue("aes-key", isSingle: true, ResValueFormat.String),
                        RSASecret = GetResValue("rsa-public-key", isSingle: false, ResValueFormat.String),
#if __MOBILE__
                        MASLClientId = GetResValueGuid("masl-client-id", isSingle: true, ResValueFormat.StringGuidN),
#endif
                    };
                    SetApiBaseUrl(mAppSettings);
#if StartupTrace
                    stopwatch.Stop();
                    Console.WriteLine($"Load AppSettings, value: {stopwatch.ElapsedMilliseconds}");
#endif
                    static Guid GetResValueGuid(string name, bool isSingle, ResValueFormat format) => GetResValue(name, isSingle, format).TryParseGuidN() ?? default;
                    static string? GetResValue(string name, bool isSingle, ResValueFormat format)
                    {
                        const string namespacePrefix = "System.Application.UI.Resources.";
                        var assembly = Assembly.GetExecutingAssembly();
                        Stream? func(string x) => assembly.GetManifestResourceStream(x);
                        var r = AppClientAttribute.GetResValue(func, name, isSingle, namespacePrefix, format);
                        return r;
                    }
                    static void SetApiBaseUrl(AppSettings s)
                    {
                        //#if DEBUG
                        //                        if (BuildConfig.IsAigioPC && Program.IsMainProcess)
                        //                        {
                        //                            try
                        //                            {
                        //                                var url = Prefix_HTTPS + "localhost:5001";
                        //                                var request = WebRequest.CreateHttp(url);
                        //                                request.Timeout = 1888;
                        //                                request.GetResponse();
                        //                                s.ApiBaseUrl = url;
                        //                                return;
                        //                            }
                        //                            catch (Exception e)
                        //                            {
                        //                                Debug.WriteLine(e.ToString());
                        //                            }
                        //                        }
                        //#endif
                        var value =
                            (_ThisAssembly.Debuggable || !s.GetIsOfficialChannelPackage()) ?
                            Prefix_HTTPS + "pan.mossimo.net:8862" :
                            Prefix_HTTPS + "api.steampp.net";
                        s.ApiBaseUrl = value;
                    }
                }
                return mAppSettings;
            }
        }
#endif

#if !__MOBILE__ && !CONSOLEAPP
        public static bool HasNotifyIcon { get; private set; }
#endif

#if UI_DEMO
        sealed class MockServiceProvider : IServiceProvider
        {
            static readonly Type typeMock = typeof(Mock<>);
            readonly Dictionary<Type, object?> pairs = new();
            readonly IServiceProvider serviceProvider;

            public MockServiceProvider(Action<IServiceCollection> configureServices)
            {
                var services = new ServiceCollection();
                configureServices(services);
                serviceProvider = services.BuildServiceProvider();
            }

            public object? GetService(Type serviceType)
            {
                var service = serviceProvider.GetService(serviceType);
                if (service != null) return service;
                if (pairs.ContainsKey(serviceType)) return pairs[serviceType];
                var mockServiceType = typeMock.MakeGenericType(serviceType);
                var mockService = (Mock?)Activator.CreateInstance(mockServiceType);
                service = mockService?.Object;
                pairs.Add(serviceType, service);
                return service;
            }
        }
#endif

#if !CONSOLEAPP
        /// <inheritdoc cref="IActiveUserClient.Post(ActiveUserRecordDTO, Guid?)"/>
        internal static async void ActiveUserPost(ActiveUserType type)
        {
            if (!Program.IsMainProcess) return;
            try
            {
#if !__MOBILE__
                var screens = App.Instance.MainWindow.Screens;
#else
                var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
                var mainDisplayInfoH = mainDisplayInfo.Height.ToInt32(NumberToInt32Format.Ceiling);
                var mainDisplayInfoW = mainDisplayInfo.Width.ToInt32(NumberToInt32Format.Ceiling);
#endif
                var req = new ActiveUserRecordDTO
                {
                    Type = type,
#if __MOBILE__
                    ScreenCount = 1,
                    PrimaryScreenPixelDensity = mainDisplayInfo.Density,
                    PrimaryScreenWidth = mainDisplayInfoW,
                    PrimaryScreenHeight = mainDisplayInfoH,
                    SumScreenWidth = mainDisplayInfoW,
                    SumScreenHeight = mainDisplayInfoH,
#else
                    ScreenCount = screens.ScreenCount,
                    PrimaryScreenPixelDensity = screens.Primary.PixelDensity,
                    PrimaryScreenWidth = screens.Primary.Bounds.Width,
                    PrimaryScreenHeight = screens.Primary.Bounds.Height,
                    SumScreenWidth = screens.All.Sum(x => x.Bounds.Width),
                    SumScreenHeight = screens.All.Sum(x => x.Bounds.Height),
#endif
                };
                Guid? lastNotificationRecordId = default;
                if (type == ActiveUserType.OnStartup)
                {
                    lastNotificationRecordId = await INotificationService.GetLastNotificationRecordId();
                }
                var rsp = await ICloudServiceClient.Instance.ActiveUser.Post(req, lastNotificationRecordId);
                INotificationService.Notify(rsp, type);
            }
            catch (Exception e)
            {
                Log.Error(nameof(Startup), e, "ActiveUserPost");
            }
        }
#endif

        public static void OnStartup(bool isMainProcess)
        {
            if (isMainProcess)
            {
                ActiveUserPost(ActiveUserType.OnStartup);
                if (GeneralSettings.IsAutoCheckUpdate.Value)
                {
                    IApplicationUpdateService.Instance.CheckUpdate(showIsExistUpdateFalse: false);
                }
            }
        }
    }

#if StartupTrace
    /// <summary>
    /// 启动耗时跟踪
    /// </summary>
    static class StartupTrace
    {
        static Stopwatch? sw;

        public static void Restart(string? mark = null)
        {
            if (sw != null)
            {
                sw.Stop();
                var args = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).Take(1));
                var msg = $"{(string.IsNullOrWhiteSpace(args) ? "" : args + " ")}mark: {mark}, value: {sw.ElapsedMilliseconds}";
                Debug.WriteLine(msg);
                Console.WriteLine(msg);
                sw.Restart();
            }
            else
            {
                sw = Stopwatch.StartNew();
            }
        }
    }
#endif
}