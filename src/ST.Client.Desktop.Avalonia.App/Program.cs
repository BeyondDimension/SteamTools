using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

#if WINDOWS_DESKTOP_BRIDGE
[assembly: SupportedOSPlatform("Windows10.0.17763.0")]
#endif

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
        static int Main(string[] args)
        {
#if WINDOWS_DESKTOP_BRIDGE
            if (!DesktopBridgeHelper.Init()) return 0;
            DesktopBridgeHelper.OnActivated(ref args);
#elif !__MOBILE__
#if MAC
            AppDelegate.Init(/*args*/);
            FileSystemDesktopMac.InitFileSystem();
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

            AppHelper.InitLogDir();
#if StartupTrace
            StartupTrace.Restart("InitLogDir");
#endif

            //void InitCefNetApp() => CefNetApp.Init(AppHelper.LogDirPath, args);
            Startup.InitGlobalExceptionHandler();
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
                return CommandLineTools.Run(args_clt);
            }
            catch (Exception ex)
            {
                Startup.GlobalExceptionHandler(ex, nameof(Main));
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

        /// <summary>
        /// 是否最小化启动
        /// </summary>
        public static bool IsMinimize { get; set; }

        /// <summary>
        /// 当前是否是命令行工具进程
        /// </summary>
        public static bool IsCLTProcess { get; private set; }

        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        public static bool IsMainProcess { get; private set; }

        /// <summary>
        /// 当前是否是用于托盘的独立进程
        /// </summary>
        public static bool IsTrayProcess { get; private set; }
    }
}