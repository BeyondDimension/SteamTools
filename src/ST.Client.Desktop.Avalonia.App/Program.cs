using NLog;
using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.ExceptionServices;
using AvaloniaApplication = Avalonia.Application;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace System.Application.UI
{
    static partial class Program
    {
        static readonly HashSet<Exception> exceptions = new();
        static readonly object lock_global_ex_log = new();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        [HandleProcessCorruptedStateExceptions]
        static int Main(string[] args)
        {
#if WINDOWS_DESKTOP_BRIDGE
            if (!DesktopBridgeHelper.Init()) return 0;
#elif !__MOBILE__
#if MAC
            AppDelegateHelper.Init(args);
            //FileSystemDesktopMac.InitFileSystem();
            FileSystemDesktop.InitFileSystem();
#else
            FileSystemDesktop.InitFileSystem();
#endif
#endif
#if StartupTrace
            StartupTrace.Restart();
#endif
            // 目前桌面端默认使用 SystemTextJson 如果出现兼容性问题可取消下面这行代码
            // Serializable.DefaultJsonImplType = Serializable.JsonImplType.NewtonsoftJson;
            IsMainProcess = args.Length == 0;
            IsCLTProcess = !IsMainProcess && args.FirstOrDefault() == "-clt";

            var logDirPath = InitLogDir();
#if StartupTrace
            StartupTrace.Restart("InitLogDir");
#endif

            void InitCefNetApp() => CefNetApp.Init(logDirPath, args);
            void InitAvaloniaApp() => BuildAvaloniaAppAndStartWithClassicDesktopLifetime(args);
            void InitStartup(DILevel level) => Startup.Init(level);
            logger = LogManager.GetCurrentClassLogger();
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    HandleGlobalException(ex, nameof(AppDomain.UnhandledException), e.IsTerminating);
                }
            };
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
            {
                // https://github.com/AvaloniaUI/Avalonia/issues/5290#issuecomment-760751036
                HandleGlobalException(ex, nameof(RxApp.DefaultExceptionHandler));
            });
            try
            {
                string[] args_clt;
                if (IsCLTProcess) // 命令行模式
                {
                    args_clt = args.Skip(1).ToArray();
                    if (args_clt.Length == 1 && args_clt[0].Equals(command_main, StringComparison.OrdinalIgnoreCase)) return default;
                }
                else
                {
                    args_clt = new[] { command_main };
                }
                return CommandLineTools.Run(args_clt, InitStartup, InitAvaloniaApp, InitCefNetApp);
            }
            catch (Exception ex)
            {
                HandleGlobalException(ex, nameof(Main));
                throw;
            }
            finally
            {
                appInstance?.Dispose();
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
                ArchiSteamFarm.LogManager.Shutdown();
            }

            static void HandleGlobalException(Exception ex, string name, bool? isTerminating = null)
            {
                lock (lock_global_ex_log)
                {
                    if (exceptions.Contains(ex)) return;
                    exceptions.Add(ex);
                }

                AppHelper.TrySetLoggerMinLevel(LogLevel.Trace);
                // NLog: catch any exception and log it.
                logger?.Error(ex, "Stopped program because of exception, name: {1}, isTerminating: {0}", isTerminating, name);

                try
                {
                    DI.Get<IHttpProxyService>().StopProxy();
                    ProxyService.OnExitRestoreHosts();
                }
                catch (Exception ex_restore_hosts)
                {
                    logger?.Error(ex_restore_hosts, "(App)Close exception when OnExitRestoreHosts");
                }

                var callTryShutdown = true;
                try
                {
                    callTryShutdown = !App.Shutdown();
                }
                catch (Exception ex_shutdown)
                {
                    logger?.Error(ex_shutdown,
                        "(App)Close exception when exception occurs");
                }

                if (callTryShutdown)
                {
                    try
                    {
                        AppHelper.TryShutdown();
                    }
                    catch (Exception ex_shutdown_app_helper)
                    {
                        logger?.Error(ex_shutdown_app_helper,
                            "(AppHelper)Close exception when exception occurs");
                    }
                }

                try
                {
                    if (AvaloniaApplication.Current is App app)
                    {
                        app.compositeDisposable.Dispose();
                    }
                }
                catch
                {
                }

#if DEBUG
                //MessageBoxCompat.Show(ex.ToString(), "Steam++ Run Error" + ThisAssembly.Version, MessageBoxButtonCompat.OK, MessageBoxImageCompat.Warning);
#endif

#if DEBUG && WINDOWS
                if (ex.InnerException is BadImageFormatException)
                {
                    typeof(Program).Assembly.ManifestModule.GetPEKind(out var peKind, out var _);
                    if (!peKind.HasFlag(Reflection.PortableExecutableKinds.Required32Bit))
                    {
                        logger?.Error("Some references need to work on X86 platforms.");
                    }
                }
#endif
            }
        }

        /// <summary>
        /// 是否最小化启动
        /// </summary>
        public static bool IsMinimize { get; /*private*/ set; }

        /// <summary>
        /// 当前是否是命令行工具进程
        /// </summary>
        public static bool IsCLTProcess { get; private set; }

        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        public static bool IsMainProcess { get; private set; }
    }
}