using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLogLevel = NLog.LogLevel;

namespace System.Application.UI
{
    static partial class Program
    {
        static Logger? logger;
        static readonly HashSet<Exception> exceptions = new();
        static readonly object lock_global_ex_log = new();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        static int Main(string[] args)
        {
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
                if (IsCLTProcess) // 命令行模式
                {
                    var args_clt = args.Skip(1).ToArray();
                    return CommandLineTools.Main(args_clt, InitStartup, InitAvaloniaApp, InitCefNetApp);
                }
                else
                {
#if StartupTrace
                    StartupTrace.Restart("ProcessCheck");
#endif

                    Startup.Init(IsMainProcess ? DILevel.MainProcess : DILevel.Min);
#if StartupTrace
                    StartupTrace.Restart("Startup.Init");
#endif
                    if (IsMainProcess)
                    {
                        var appInstance = new ApplicationInstance();
                        if (!appInstance.IsFirst) goto exit;
                    }
#if StartupTrace
                    StartupTrace.Restart("ApplicationInstance");
#endif
                    InitCefNetApp();
#if StartupTrace
                    StartupTrace.Restart("InitCefNetApp");
#endif

                    if (IsMainProcess)
                    {
                        InitAvaloniaApp();
                    }
#if StartupTrace
                    StartupTrace.Restart("InitAvaloniaApp");
#endif
                }
            }
            catch (Exception ex)
            {
                HandleGlobalException(ex, nameof(Main));
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }

        exit: return 0;

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
                    IHostsFileService.OnExitRestoreHosts();
                }
                catch (Exception ex_restore_hosts)
                {
                    logger?.Error(ex_restore_hosts, "(App)Close exception when OnExitRestoreHosts");
                }

                try
                {
                    if (!App.Shutdown())
                    {
                        try
                        {
                            AppHelper.Shutdown?.Invoke();
                        }
                        catch (Exception ex_shutdown_app_helper)
                        {
                            logger?.Error(ex_shutdown_app_helper,
                                "(AppHelper)Close exception when exception occurs");
                        }
                    }
                }
                catch (Exception ex_shutdown)
                {
                    logger?.Error(ex_shutdown,
                        "(App)Close exception when exception occurs");
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

        static string InitLogDir()
        {
            var logDirPath = Path.Combine(AppContext.BaseDirectory, "Logs");
            IOPath.DirCreateByNotExists(logDirPath);
#if StartupTrace
            StartupTrace.Restart("InitLogDir.IO");
#endif
            var logDirPath_ = logDirPath + Path.DirectorySeparatorChar;

            InternalLogger.LogFile = logDirPath_ + "internal-nlog.txt";
            InternalLogger.LogLevel = NLogLevel.Error;
            var objConfig = new LoggingConfiguration();
            var logfile = new FileTarget("logfile")
            {
                FileName = logDirPath_ + "nlog-all-${shortdate}.log",
                Layout = "${longdate}|${level}|${logger}|${message} |${all-event-properties} ${exception:format=tostring}",
            };
            objConfig.AddTarget(logfile);
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, logfile, "Microsoft.*");
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, logfile, "System.Net.Http.*");
            objConfig.AddRule(AppHelper.DefaultNLoggerMinLevel, NLogLevel.Fatal, logfile, "*");
#if StartupTrace
            StartupTrace.Restart("InitLogDir.CreateLoggingConfiguration");
#endif
            LogManager.Configuration = objConfig;

            return logDirPath;
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