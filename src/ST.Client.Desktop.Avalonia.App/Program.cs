using System.Linq;
using System.Net;
using System.Runtime.Versioning;
using System.Reflection;
using System.Threading.Tasks;
using System.Application.Settings;
using System.Application.Services;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using NLog;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Controls;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

#if MAC
[assembly: SupportedOSPlatform("macOS")]
#elif LINUX
[assembly: SupportedOSPlatform("Linux")]
#elif WINDOWS_DESKTOP_BRIDGE
//using Microsoft.Toolkit.Uwp.Notifications;
#pragma warning disable SA1516 // Elements should be separated by blank line
#if DEBUG && !MSIX_SINGLE_PROJECT
using WinFormsMessageBox = System.Windows.Forms.MessageBox;
#endif
[assembly: SupportedOSPlatform("Windows10.0.17763.0")]
#pragma warning restore SA1516 // Elements should be separated by blank line
#elif WINDOWS
[assembly: SupportedOSPlatform("Windows7.0")]
#endif

namespace System.Application.UI
{
    static partial class Program
    {
        const string command_main = "main";
        static IApplication.SingletonInstance? appInstance;
        static readonly ProgramHost host = new();

        [STAThread]
        static int Main(string[] args)
        {
            // fix The request was aborted: Could not create SSL/TLS secure channel
            TrySetSecurityProtocol();

            host.IsMainProcess = args.Length == 0;
            host.IsCLTProcess = !host.IsMainProcess && args.FirstOrDefault() == "-clt";

#if WINDOWS_DESKTOP_BRIDGE
            if (!DesktopBridgeHelper.Init()) return 0;
            InitWithUWP(args);
#elif MAC
            InitWithMAC(args);
#endif
            OnCreateAppExecuting();
            // InitCefNetApp(args);

            try
            {
                string[] args_clt;
                if (host.IsCLTProcess) // 命令行模式
                {
                    args_clt = args.Skip(1).ToArray();
                    if (args_clt.Length == 1 && args_clt[0].Equals(command_main, StringComparison.OrdinalIgnoreCase)) return default;
                }
                else
                {
                    args_clt = new[] { command_main };
                }
                return CommandLineTools.Run(args_clt);
            }
            catch (Exception ex)
            {
                GlobalExceptionHelpers.Handler(ex, nameof(Main));
                throw;
            }
            finally
            {
                appInstance?.Dispose();
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
                ArchiSteamFarm.LogManager.Shutdown();
            }
        }

        static void TrySetSecurityProtocol(SecurityProtocolType type = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13)
        {
            try
            {
                ServicePointManager.SecurityProtocol = type;
            }
            catch (NotSupportedException)
            {

            }
        }

#if WINDOWS_DESKTOP_BRIDGE
#if DEBUG && !MSIX_SINGLE_PROJECT
        static void ShowArgs(string[] args) => WinFormsMessageBox.Show(string.Join(' ', args), "Main(string[] args)");
#endif

        static void InitWithUWP(string[] args)
        {
#if DEBUG && !MSIX_SINGLE_PROJECT
            ShowArgs(args);
#endif
            // RegisterToastNotificationManager();
            DesktopBridgeHelper.OnActivated(ref args);
        }

        //static void RegisterToastNotificationManager()
        //{
        //    if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
        //    {
        //        // 通过通知中心点击通知启动的进程
        //        args = Array.Empty<string>();
        //        //Handle when activated by click on notification
        //        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        //        {
        //            //Get the activation args, if you need those.
        //            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
        //            //Get user input if there's any and if you need those.
        //            var userInput = toastArgs.UserInput;
        //            //if the app instance just started after clicking on a notification 
        //            if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
        //            {
        //                System.Windows.Forms.MessageBox.Show("App was not running, " +
        //                       "but started and activated by click on a notification.");
        //            }
        //            else
        //            {
        //                System.Windows.Forms.MessageBox.Show("App was running, " +
        //                    "and activated by click on a notification.");
        //            }
        //        };

        //        Console.ReadLine();
        //        while (true)
        //        {

        //        }
        //    }
        //}
#elif MAC
        static void InitWithMAC(string[] args)
        {
            AppDelegate.Init(/*args*/);
        }
#endif

        private sealed class ProgramHost : IApplication.IDesktopProgramHost
        {
            public bool IsMinimize { get; set; }

            public bool IsCLTProcess { get; set; }

            public bool IsMainProcess { get; set; }

            public bool IsTrayProcess { get; set; }

            public void ConfigureServices(DILevel level) => Program.ConfigureServices(this, level);

            public void InitVisualStudioAppCenterSDK()
            {
#if WINDOWS || XAMARIN_MAC || __MOBILE__ || __ANDROID__ || __IOS__
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable CA1416 // 验证平台兼容性
                VisualStudioAppCenterSDK.Init();
#pragma warning restore CA1416 // 验证平台兼容性
#pragma warning restore IDE0079 // 请删除不必要的忽略
#endif
            }

            public void OnStartup() => Program.OnStartup(this);

            IApplication IApplication.IProgramHost.Application => App.Instance;

            void IApplication.IDesktopProgramHost.OnCreateAppExecuted(Action<IViewModelManager>? handlerViewModelManager, bool isTrace) => Program.OnCreateAppExecuted(this, handlerViewModelManager, isTrace);
        }

        //static void InitCefNetApp(string[] args) => CefNetApp.Init(AppHelper.LogDirPath, args);

        /// <summary>
        /// Avalonia configuration, don't remove; also used by visual designer.
        /// </summary>
        /// <returns></returns>
        static AppBuilder BuildAvaloniaApp()
        {
#if DEBUG
            // 设计器模式不会执行 Main 函数所以以此区分来初始化文件系统
            if (Design.IsDesignMode)
            //if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
            {
                OnCreateAppExecuting();
            }
#endif
            var builder = AppBuilder.Configure(() => new App(host))
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();

            var useGpu = !IApplication.DisableGPU && GeneralSettings.UseGPURendering.Value;

#if MAC
            builder.With(new AvaloniaNativePlatformOptions
            {
                UseGpu = useGpu
            });
#elif LINUX
            builder.With(new X11PlatformOptions
            {
                UseGpu = useGpu
            });
#elif WINDOWS
            var useWgl = IApplication.UseWgl || GeneralSettings.UseWgl.Value;
            var options = new Win32PlatformOptions
            {
                UseWgl = useWgl,
                AllowEglInitialization = useGpu,
            };
            builder.With(options);

            var skiaOptions = new SkiaOptions
            {
                MaxGpuResourceSizeBytes = 1024000000,
            };

            builder.With(skiaOptions);
#else
            throw new PlatformNotSupportedException();
#endif

            return builder;
        }

        static void StartAvaloniaApp(string[] args, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
        {
            var builder = BuildAvaloniaApp();
            builder.StartWithClassicDesktopLifetime(args, shutdownMode);
        }

        /// <summary>
        /// 命令行工具(Command Line Tools/CLT)
        /// </summary>
        static class CommandLineTools
        {
            public static int Run(string[] args)
            {
                if (args.Length == 0) args = new[] { "-h" };

                // https://docs.microsoft.com/zh-cn/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline
                var rootCommand = new RootCommand("命令行工具(Command Line Tools/CLT)");

                void MainHandler() => MainHandler_(null);
                void MainHandler_(Action? onInitStartuped)
                {
#if StartWatchTrace
                    StartWatchTrace.Record("ProcessCheck");
#endif
                    ConfigureServices(host, host.IsMainProcess ? DILevel.MainProcess : DILevel.Min);
#if StartWatchTrace
                    StartWatchTrace.Record("Startup.Init");
#endif
                    onInitStartuped?.Invoke();
                    if (host.IsMainProcess)
                    {
                        var isInitAppInstanceReset = false;
                    initAppInstance: appInstance = new();
                        if (!appInstance.IsFirst)
                        {
                            //Console.WriteLine("ApplicationInstance.SendMessage(string.Empty);");
                            if (IApplication.SingletonInstance.SendMessage(string.Empty))
                            {
                                return;
                            }
                            else
                            {
                                if (!isInitAppInstanceReset &&
                                    IApplication.SingletonInstance.TryKillCurrentAllProcess())
                                {
                                    isInitAppInstanceReset = true;
                                    appInstance.Dispose();
                                    goto initAppInstance;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        appInstance.MessageReceived += value =>
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                var app = App.Instance;
                                if (app != null)
                                {
                                    MainThread2.BeginInvokeOnMainThread(app.RestoreMainWindow);
                                }
                            }
                        };
                    }
                    //#if StartWatchTrace
                    //                    StartWatchTrace.Record("ApplicationInstance");
                    //#endif
                    //                    initCef();
                    //#if StartWatchTrace
                    //                    StartWatchTrace.Record("InitCefNetApp");
                    //#endif
                    if (host.IsMainProcess)
                    {
                        StartAvaloniaApp(args);
                    }
#if StartWatchTrace
                    StartWatchTrace.Record("InitAvaloniaApp");
#endif
                }
                void MainHandlerByCLT() => MainHandlerByCLT_(null);
                void MainHandlerByCLT_(Action? onInitStartuped)
                {
                    host.IsMainProcess = true;
                    host.IsCLTProcess = false;
                    MainHandler_(onInitStartuped);
                }

#if DEBUG
                // -clt debug -args 730
                var debug = new Command("debug", "调试");
                debug.AddOption(new Option<string>("-args", () => "", "测试参数"));
                debug.Handler = CommandHandler.Create((string args) => // 参数名与类型要与 Option 中一致！
                {
                    //Console.WriteLine("-clt debug -args " + args);
                    // OutputType WinExe 导致控制台输入不会显示，只能附加一个新的控制台窗口显示内容，不合适
                    // 如果能取消 管理员权限要求，改为运行时管理员权限，
                    // 则可尝试通过 Windows Terminal 或直接 Host 进行命令行模式
                    MainHandlerByCLT();
                });
                rootCommand.AddCommand(debug);
#endif

                var main = new Command(command_main)
                {
                    Handler = CommandHandler.Create(MainHandler),
                };
                rootCommand.AddCommand(main);

                // -clt devtools
                // -clt devtools -disable_gpu
                // -clt devtools -use_wgl
                var devtools = new Command("devtools");
                devtools.AddOption(new Option<bool>("-disable_gpu", () => false, "禁用 GPU 硬件加速"));
                devtools.AddOption(new Option<bool>("-use_wgl", () => false, "使用 Native OpenGL(仅 Windows)"));
                devtools.Handler = CommandHandler.Create((bool disable_gpu, bool use_wgl) =>
                {
                    IApplication.EnableDevtools = true;
                    IApplication.DisableGPU = disable_gpu;
                    IApplication.UseWgl = use_wgl;
                    MainHandlerByCLT_(onInitStartuped: () =>
                    {
                        IApplication.LoggerMinLevel = LogLevel.Debug;
                    });
                });
                rootCommand.AddCommand(devtools);

                // -clt c -silence
                var common = new Command("c", "common");
                common.AddOption(new Option<bool>("-silence", "静默启动（不弹窗口）"));
                common.Handler = CommandHandler.Create((bool silence) =>
                {
                    host.IsMinimize = silence;
                    MainHandlerByCLT();
                });
                rootCommand.AddCommand(common);

                // -clt steam -account
                var steamuser = new Command("steam", "Steam 相关操作");
                steamuser.AddOption(new Option<string>("-account", "指定对应 Steam 用户名"));
                steamuser.Handler = CommandHandler.Create((string account) =>
                {
                    if (!string.IsNullOrEmpty(account))
                    {
                        ConfigureServices(host, DILevel.Steam);

                        var steamService = ISteamService.Instance;

                        var users = steamService.GetRememberUserList();

                        var currentuser = users.Where(s => s.AccountName == account).FirstOrDefault();

                        if (currentuser != null)
                        {
                            currentuser.MostRecent = true;
                            steamService.UpdateLocalUserData(users);
                            steamService.SetCurrentUser(account);
                        }

                        steamService.TryKillSteamProcess();
                        steamService.StartSteam();
                    }
                });
                rootCommand.AddCommand(steamuser);

                // -clt app -id 632360
                var unlock_achievement = new Command("app", "打开成就解锁窗口");
                unlock_achievement.AddOption(new Option<int>("-id", "指定一个 Steam 游戏 Id"));
                unlock_achievement.AddOption(new Option<bool>("-silence", "挂运行服务，不加载窗口，内存占用更小"));
                unlock_achievement.Handler = CommandHandler.Create(async (int id, bool silence) =>
                {
                    try
                    {
                        if (id <= 0) return;
                        if (!silence)
                        {
                            ConfigureServices(host, DILevel.GUI | DILevel.Steam | DILevel.HttpClientFactory);
                            IViewModelManager.Instance.InitUnlockAchievement(id);
                            StartAvaloniaApp(args);
                        }
                        else
                        {
                            ConfigureServices(host, DILevel.Steam);
                            SteamConnectService.Current.Initialize(id);
                            TaskCompletionSource tcs = new();
                            await tcs.Task;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(unlock_achievement), ex, "Start");
                    }
                });
                rootCommand.AddCommand(unlock_achievement);

                var r = rootCommand.InvokeAsync(args).Result;
                return r;
            }
        }
    }
}