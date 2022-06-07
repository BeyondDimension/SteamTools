#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name
#pragma warning disable SA1216 // Using static directives should be placed at the correct location
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#if !MAUI
#if !__MOBILE__ && !CONSOLEAPP
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
#else
using Xamarin.Essentials;
#endif
#endif
#if UI_DEMO
using Moq;
#endif
using NLog;
using System.Text;
using System.Diagnostics;
using System.Application.UI;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Application.Models;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.Security;
using System.Reflection;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Linq;
using System.IO;
//using static System.Application.AppClientAttribute;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Settings;
using System.Threading.Tasks;
#if __ANDROID__
using System.Application.UI.Resx;
using System.Windows;
using Microsoft.Extensions.Http;
using Xamarin.Android.Net;
using Program = System.Application.UI.MainApplication;
using PlatformApplication = System.Application.UI.MainApplication;
#elif __IOS__
using Program = System.Application.UI.AppDelegate;
#elif !__MOBILE__
using ReactiveUI;
using System.Reactive;
#if !MAUI
using AvaloniaApplication = Avalonia.Application;
#endif
using PlatformApplication = System.Application.UI.App;
#endif
#if StartWatchTrace
using System.Diagnostics;
#endif
#if ANDROID || IOS || __ANDROID__
#if MAUI
using EssentialsFileSystem = Microsoft.Maui.Storage.FileSystem;
#else
using EssentialsFileSystem = Xamarin.Essentials.FileSystem;
#endif
#endif
using static System.Application.Browser2;
using _ThisAssembly = System.Properties.ThisAssembly;
using _UserService = System.Application.Services.UserService;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
#pragma warning restore SA1209 // Using alias directives should be placed after other using directives
#pragma warning restore SA1216 // Using static directives should be placed at the correct location
#pragma warning restore SA1211 // Using alias directives should be ordered alphabetically by alias name
#pragma warning restore IDE0079 // 请删除不必要的忽略

namespace System.Application.UI
{
    partial class
#if MAUI
        MauiProgram
#elif __ANDROID__
        MainApplication
#elif __IOS__
        Program
#elif !__MOBILE__
        Program
#endif
    {
        #region AppSettings

#if !CONSOLEAPP
        static AppSettings? mAppSettings;

        public static AppSettings AppSettings
        {
            get
            {
                if (mAppSettings == null)
                {
#if StartWatchTrace
                    var stopwatch = Stopwatch.StartNew();
#endif
                    mAppSettings = new AppSettings
                    {
                        //AppVersion = GetResValueGuid("app-id", isSingle: false, ResSecrets.ResValueFormat.StringGuidN),
                        AesSecret = ResSecrets.GetResValue("aes-key", isSingle: true, ResSecrets.ResValueFormat.String),
                        RSASecret = ResSecrets.GetResValue("rsa-public-key", isSingle: false, ResSecrets.ResValueFormat.String),
                        //MASLClientId = GetResValueGuid("masl-client-id", isSingle: true, ResSecrets.ResValueFormat.StringGuidN),
                    };
                    SetApiBaseUrl(mAppSettings);
#if StartWatchTrace
                    stopwatch.Stop();
                    Console.WriteLine($"Load AppSettings, value: {stopwatch.ElapsedMilliseconds}");
#endif
                    //static Guid GetResValueGuid(string name, bool isSingle, ResValueFormat format) => GetResValue(name, isSingle, format).TryParseGuidN() ?? default;
                    //static string? GetResValue(string name, bool isSingle, ResValueFormat format)
                    //{
                    //    const string namespacePrefix = "System.Application.UI.Resources.";
                    //    var assembly = Assembly.GetExecutingAssembly();
                    //    Stream? func(string x) => assembly.GetManifestResourceStream(x);
                    //    var r = AppClientAttribute.GetResValue(func, name, isSingle, namespacePrefix, format);
                    //    return r;
                    //}
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
                        const string ApiBaseUrl_Development =
#if USE_DEBUG_SERVER
                            $"{Prefix_HTTPS}localhost:5001";
#else
                            $"{Prefix_HTTPS}pan.mossimo.net:8862";
#endif
                        const string ApiBaseUrl_Production = $"{Prefix_HTTPS}api.steampp.net";
                        var value =
                            (_ThisAssembly.Debuggable || !s.GetIsOfficialChannelPackage()) ?
                                ApiBaseUrl_Development :
                                ApiBaseUrl_Production;
                        s.ApiBaseUrl = value;
                    }
                }
                return mAppSettings;
            }
        }
#endif

        #endregion

        #region ConfigureServices

        static bool isInitialized;

        /// <summary>
        /// 初始化启动
        /// </summary>
        static void ConfigureServices(IApplication.IStartupArgs args, DILevel level, IServiceCollection? services = null)
        {
            if (!isInitialized)
            {
                isInitialized = true;
#if StartWatchTrace
                StartWatchTrace.Record("Startup.InitFileSystem");
#endif
                var options = new StartupOptions(level);
                if (options.HasServerApiClient)
                {
                    ModelValidatorProvider.Init();
#if StartWatchTrace
                    StartWatchTrace.Record("ModelValidatorProvider.Init");
#endif
                }
#if StartWatchTrace
                StartWatchTrace.Record($"InitDI: {level}");
#endif
                if (services == null)
                {
                    // new ServiceCollection 创建服务集合
                    ConfigureServices(args, options);
                    OnBuild();
                }
                else
                {
                    // 使用传入的 ServiceCollection
                    // 但需要在完成时调用 DI.ConfigureServices(IServiceProvider) 与 OnBuild
                    ConfigureServices(services, args, options);
                }
            }
        }

        static void OnBuild()
        {
            VersionTracking2.Track();
#if StartWatchTrace
            StartWatchTrace.Record($"VersionTracking2.Track");
#endif
            Migrations.Up();
#if StartWatchTrace
            StartWatchTrace.Record($"Migrations.Up");
#endif
        }

        static void ConfigureServices(IApplication.IStartupArgs args, StartupOptions options)
        {
#if UI_DEMO
            DI.Init(new MockServiceProvider(ConfigureDemoServices));
#else
            DI.ConfigureServices(s => ConfigureServices(s, args, options));
#endif
        }

        static void ConfigureServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
        {
            ConfigureRequiredServices(services, args, options);
#if StartWatchTrace
            StartWatchTrace.Record("DI.ConfigureRequiredServices");
#endif
            ConfigureDemandServices(services, args, options);
#if StartWatchTrace
            StartWatchTrace.Record("DI.ConfigureDemandServices");
#endif
        }

        /// <summary>
        /// 配置任何进程都必要的依赖注入服务
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureRequiredServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
        {
#if !__ANDROID__ || MAUI
            services.AddSingleton(ProgramHost.Instance);
#endif
#if !UI_DEMO
            // 平台服务 此项放在其他通用业务实现服务之前
            services.AddPlatformService(options);
#endif
#if WINDOWS
#if !MAUI
            services.AddMSAppCenterApplicationSettings();
#endif
            services.AddJumpListService();
#endif

            // 添加日志实现
            services.AddGeneralLogging();
#if MAUI || __MOBILE__ || ANDROID || IOS || __ANDROID__
            services.TryAddEssentials();
#endif
#if __MOBILE__ || ANDROID || IOS || __ANDROID__
            // 添加运行时权限
            services.AddPlatformPermissions();
#endif
#if !CONSOLEAPP
            // 添加 app 配置项
            services.TryAddOptions(AppSettings);
#if MAUI || __MOBILE__ || ANDROID || IOS || __ANDROID__
            // 键值对存储 - 由 Essentials 提供
            services.TryAddEssentialsSecureStorage();
#else
            // 键值对存储 - 由 Repository 提供
            services.TryAddRepositorySecureStorage();
            // 首选项(Preferences) - 由 Repository 提供
            services.AddRepositoryPreferences();
#endif

            // 添加安全服务
            services.AddSecurityService<EmbeddedAesDataProtectionProvider, LocalDataProtectionProvider>();
#endif
        }

        /// <summary>
        /// 配置按需使用的依赖注入服务
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureDemandServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
        {
#if StartWatchTrace
            StartWatchTrace.Record("DI.ConfigureDemandServices.Calc");
#endif
            services.AddDnsAnalysisService();
#if !CONSOLEAPP
            if (options.HasGUI)
            {
                services.AddPinyin();
#if __MOBILE__ || MAUI
                services.TryAddFontManager();
#else
                services.TryAddAvaloniaFontManager(useGdiPlusFirst: true);
#endif
                // 添加 Toast 提示服务
#if !DEBUG
                services.AddStartupToastIntercept();
#endif
                services.TryAddToast();

                services.AddSingleton(_ => PlatformApplication.Instance);

                services.AddSingleton<IApplication>(s => s.GetRequiredService<PlatformApplication>());
#if __ANDROID__
                services.AddSingleton<IAndroidApplication>(s => s.GetRequiredService<PlatformApplication>());
#endif
#if __MOBILE__
                // 添加电话服务
                services.AddTelephonyService();

                //services.AddMSALPublicClientApp(AppSettings.MASLClientId);
#elif MAUI
                services.AddSingleton<IMauiApplication>(s => s.GetRequiredService<PlatformApplication>());
#else
                services.AddSingleton<IAvaloniaApplication>(s => s.GetRequiredService<PlatformApplication>());
                services.TryAddSingleton<IClipboardPlatformService>(s => s.GetRequiredService<PlatformApplication>());

                // 添加主线程助手(MainThreadDesktop)
                services.AddMainThreadPlatformService();

                services.TryAddAvaloniaFilePickerPlatformService();
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
                services.TryAddWindowManager();

#if WINDOWS
                // 可选项，在 Win 平台使用 WPF 实现的 MessageBox
                //services.AddSingleton<IMessageBoxCompatService, WPFMessageBoxCompatService>();
#endif

                #endregion

                // 添加管理主窗口服务
                services.AddViewModelManager();

                services.TryAddBiometricService();
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.GUI");
#endif
            }
#endif
            if (options.HasHttpClientFactory
#if !CONSOLEAPP
                || options.HasServerApiClient
#endif
                )
            {
                // 添加 Http 平台助手桌面端或移动端实现
                services.TryAddClientHttpPlatformHelperService();
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.ClientHttpPlatformHelperService");
#endif
            }

            if (options.HasHttpClientFactory)
            {
#if __MOBILE__
                // 添加 HttpClientFactory 平台原生实现
                services.AddNativeHttpClient();
#endif
                // 通用 Http 服务
                services.AddHttpService();
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.HttpClientFactory");
#endif
            }
            services.TryAddScriptManager();
#if StartWatchTrace
            StartWatchTrace.Record("DI.ConfigureDemandServices.ScriptManager");
#endif

#if !CONSOLEAPP
            if (options.HasHttpProxy)
            {
                // 通用 Http 代理服务
                services.AddHttpProxyService();
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.HttpProxy");
#endif
            }
#endif
#if !CONSOLEAPP
            if (options.HasServerApiClient)
            {
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.AppSettings");
#endif
                // 添加模型验证框架
                services.TryAddModelValidator();

                // 添加服务端API调用
#pragma warning disable SA1111 // Closing parenthesis should be on line of last parameter
                services.TryAddCloudServiceClient<CloudServiceClient>(c =>
                {
#if NETCOREAPP3_0_OR_GREATER
                    c.DefaultRequestVersion = HttpVersion.Version30;
#endif
                }, configureHandler:
#if NETCOREAPP2_1_OR_GREATER
                () => new SocketsHttpHandler
                {
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip,
                }
#elif __ANDROID__
                () =>
                {
                    var handler = PlatformHttpMessageHandlerBuilder.CreateAndroidClientHandler();
                    handler.UseCookies = false;
                    handler.AutomaticDecompression = DecompressionMethods.GZip;
                    return handler;
                }
#else
                null
#endif
                );
#pragma warning restore SA1111 // Closing parenthesis should be on line of last parameter

                services.AddAutoMapper();

                // 添加仓储服务
                services.AddRepositories();

                // 业务平台用户管理
                services.TryAddUserManager();
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.ServerApiClient");
#endif
            }
#endif
#if !CONSOLEAPP
            // 添加通知服务
            AddNotificationService();
#if StartWatchTrace
            StartWatchTrace.Record("DI.ConfigureDemandServices.AddNotificationService");
#endif
            void AddNotificationService()
            {
#if !__MOBILE__
                if (!args.IsMainProcess) return;
#endif
                services.TryAddNotificationService();
            }
#endif
#if !CONSOLEAPP
            if (options.HasHosts)
            {
                // hosts 文件助手服务
                services.AddHostsFileService();
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.HostsFileService");
#endif
            }
#endif
            if (options.HasSteam)
            {
#if !__ANDROID__
                // Steam 相关助手、工具类服务
                services.AddSteamService();
#endif

                // Steamworks LocalApi Service
                services.TryAddSteamworksLocalApiService();

                // SteamDb WebApi Service
                //services.AddSteamDbWebApiService();

                // Steamworks WebApi Service
                services.AddSteamworksWebApiService();

                // SteamGridDB WebApi Service
                services.AddSteamGridDBWebApiService();

                // ASF Service
                services.AddArchiSteamFarmService();
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.Steam");
#endif
            }
#if !CONSOLEAPP
            if (options.HasMainProcessRequired)
            {
                // 应用程序更新服务
#if !MAUI
                services.AddApplicationUpdateService();
#else
                Console.WriteLine("TODO: AddApplicationUpdateService");
#endif
#if StartWatchTrace
                StartWatchTrace.Record("DI.ConfigureDemandServices.AppUpdateService");
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

        #endregion

        // OnCreateAppExecuting -> OnCreateAppExecuted -> DI.ConfigureServices(Init) -> OnStartup

        #region OnCreateApplication

        /// <summary>
        /// 在创建 App 前执行的初始化
        /// </summary>
        /// <param name="isTrace"></param>
        static void OnCreateAppExecuting(bool isTrace = false)
        {
            bool isDesignMode =
#if !(__MOBILE__ || MAUI)
                Design.IsDesignMode;
#else
                false;
#endif

            if (isTrace) StartWatchTrace.Record();
            try
            {
#if MAC
                FileSystemDesktopMac.InitFileSystem();
#elif LINUX
                FileSystemDesktopXDG.InitFileSystem();
#elif WINDOWS
                FileSystemDesktopWindows.InitFileSystem();
#elif ANDROID || IOS || __ANDROID__
                FileSystemEssentials.InitFileSystem();
#else
                FileSystem2.InitFileSystem();
#endif
                if (isTrace) StartWatchTrace.Record("FileSystem");

#if StartWatchTrace
                StartWatchTrace.Record();
#endif
                IApplication.InitLogDir();
#if StartWatchTrace
                StartWatchTrace.Record("InitLogDir");
#endif
                if (isTrace) StartWatchTrace.Record("NLog");

                GlobalExceptionHelpers.Init();
                if (isTrace) StartWatchTrace.Record("ExceptionHandler");

                if (!isDesignMode)
                {
                    SettingsHost.Load();
                    if (isTrace) StartWatchTrace.Record("SettingsHost");
                }
            }
            finally
            {
                if (isTrace) StartWatchTrace.Record(dispose: true);
            }
        }

        /// <summary>
        /// 在创建 App 时执行的初始化
        /// </summary>
        /// <param name="host"></param>
        /// <param name="handlerViewModelManager"></param>
        /// <param name="isTrace"></param>
        static void OnCreateAppExecuted(IApplication.IProgramHost host, Action<IViewModelManager>? handlerViewModelManager = null, bool isTrace = false)
        {
            if (isTrace) StartWatchTrace.Record();
            try
            {
                host.Application.PlatformInitSettingSubscribe();
                if (isTrace) StartWatchTrace.Record("SettingSubscribe");

                var vmService = IViewModelManager.Instance;
                vmService.InitViewModels();
                handlerViewModelManager?.Invoke(vmService);
                vmService.MainWindow.Initialize();
                if (isTrace) StartWatchTrace.Record("ViewModels");
            }
            finally
            {
                if (isTrace) StartWatchTrace.Record(dispose: true);
            }
        }

        #endregion

        #region OnStartup

        static void OnStartup(IApplication.IProgramHost host, bool isTrace = false)
        {
            if (isTrace) StartWatchTrace.Record();

            host.InitVisualStudioAppCenterSDK();
            if (isTrace) StartWatchTrace.Record("AppCenter");

            StartupToastIntercept.OnStartuped();

            if (host.IsMainProcess)
            {
                OnStartupInMainProcessAsyncVoid();
                async void OnStartupInMainProcessAsyncVoid()
                {
                    await Task.Run(() =>
                    {
                        ActiveUserPost(host, ActiveUserType.OnStartup);
                        if (GeneralSettings.IsAutoCheckUpdate.Value)
                        {
                            IApplicationUpdateService.Instance.CheckUpdate(showIsExistUpdateFalse: false);
                        }
                    });
                }

                INotificationService.ILifeCycle.Instance?.OnStartup();
            }
        }

#if !CONSOLEAPP
        /// <inheritdoc cref="IActiveUserClient.Post(ActiveUserRecordDTO)"/>
        static async void ActiveUserPost(IApplication.IStartupArgs args, ActiveUserType type)
        {
            if (!args.IsMainProcess) return;
            try
            {
                var userService = _UserService.Current;
                var isAuthenticated = userService.IsAuthenticated;
                var csc = ICloudServiceClient.Instance;
                if (isAuthenticated)
                {
                    // 刷新用户信息
                    var rspRUserInfo = await csc.Manage.RefreshUserInfo();
                    if (rspRUserInfo.IsSuccess && rspRUserInfo.Content != null)
                    {
                        await userService.SaveUserAsync(rspRUserInfo.Content);
                    }
                }
#if !__MOBILE__ && !MAUI
                var screens = PlatformApplication.Instance.MainWindow!.Screens;
#else
                var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
                var mainDisplayInfoH = mainDisplayInfo.Height.ToInt32(NumberToInt32Format.Ceiling);
                var mainDisplayInfoW = mainDisplayInfo.Width.ToInt32(NumberToInt32Format.Ceiling);
#endif
                var req = new ActiveUserRecordDTO
                {
                    Type = type,
#if __MOBILE__ || MAUI
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
                    IsAuthenticated = isAuthenticated,
                };
                req.SetDeviceId();
                // 匿名统计与通知公告
                await csc.ActiveUser.Post(req);
#if DEBUG
                INotificationService.Notify(type);
#endif
            }
            catch (Exception e)
            {
                Log.Error("Startup", e, "ActiveUserPost");
            }
        }
#endif

        #endregion

        #region GlobalExceptionHelpers

        /// <summary>
        /// 全局异常助手类
        /// </summary>
        private static class GlobalExceptionHelpers
        {
            static readonly HashSet<Exception> exceptions = new();
            static readonly object lock_global_ex_log = new();
            static readonly Lazy<Logger> _logger = new(LogManager.GetCurrentClassLogger);

            static Logger? Logger => _logger.Value;

#if __ANDROID__
            sealed class UncaughtExceptionHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
            {
                readonly Action<Java.Lang.Thread, Java.Lang.Throwable> action;
                readonly Java.Lang.Thread.IUncaughtExceptionHandler? @interface;

                public UncaughtExceptionHandler(Action<Java.Lang.Thread, Java.Lang.Throwable> action, Java.Lang.Thread.IUncaughtExceptionHandler? @interface = null)
                {
                    this.action = action;
                    this.@interface = @interface;
                }

                public void UncaughtException(Java.Lang.Thread t, Java.Lang.Throwable e)
                {
                    @interface?.UncaughtException(t, e);
                    action(t, e);
                }
            }
#endif

            /// <summary>
            /// 初始化全局异常处理
            /// </summary>
            public static void Init()
            {
#if __ANDROID__
                Java.Lang.Thread.DefaultUncaughtExceptionHandler = new UncaughtExceptionHandler((_, ex) =>
                {
                    Handler(ex, nameof(Java));
                }, Java.Lang.Thread.DefaultUncaughtExceptionHandler);
#else
                AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        Handler(ex, nameof(AppDomain), e.IsTerminating);
                    }
                };
                RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
                {
                    // https://github.com/AvaloniaUI/Avalonia/issues/5290#issuecomment-760751036
                    Handler(ex, nameof(RxApp));
                });
#endif
            }

            /// <summary>
            /// 全局异常处理
            /// </summary>
            /// <param name="ex"></param>
            /// <param name="name"></param>
            /// <param name="isTerminating"></param>
            public static void Handler(Exception ex, string name, bool? isTerminating = null)
            {
                lock (lock_global_ex_log)
                {
                    if (!exceptions.Add(ex)) return;
                }

                IApplication.TrySetLoggerMinLevel(LogLevel.Trace);
                // NLog: catch any exception and log it.
                Logger?.Error(ex, "Stopped program because of exception, name: {1}, isTerminating: {0}", isTerminating, name);

#if !__MOBILE__
                try
                {
                    DI.Get_Nullable<IHttpProxyService>()?.StopProxy();
                    ProxyService.OnExitRestoreHosts();
                }
                catch (Exception ex_restore_hosts)
                {
                    Logger?.Error(ex_restore_hosts, "(App)Close exception when OnExitRestoreHosts");
                }
#endif

#if !__MOBILE__
                //var callTryShutdown = true;
                try
                {
                    /*callTryShutdown = !*/
                    PlatformApplication.Instance?.Shutdown();
                }
                catch (Exception ex_shutdown)
                {
                    Logger?.Error(ex_shutdown,
                        "(App)Close exception when exception occurs");
                }

                //if (callTryShutdown)
                //{
                //    try
                //    {
                //        AppHelper.TryShutdown();
                //    }
                //    catch (Exception ex_shutdown_app_helper)
                //    {
                //        Logger?.Error(ex_shutdown_app_helper,
                //            "(AppHelper)Close exception when exception occurs");
                //    }
                //}

                try
                {
                    if (PlatformApplication.Instance is IApplication app)
                    {
                        app.CompositeDisposable.Dispose();
                    }
                }
                catch
                {
                }
#endif
            }
        }

        #endregion

        #region FileSystemEssentials

#if ANDROID || IOS || __ANDROID__
        sealed class FileSystemEssentials : IOPath.FileSystemBase
        {
            public static void InitFileSystem()
            {
                InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
                string GetAppDataDirectory() => EssentialsFileSystem.AppDataDirectory;
                string GetCacheDirectory() => EssentialsFileSystem.CacheDirectory;
            }
        }
#endif

        #endregion
    }
}