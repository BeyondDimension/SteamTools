#pragma warning disable SA1516 // Elements should be separated by blank line
using NLog;
using System.Linq;
using System.Net;
using System.Runtime.Versioning;

#if MAC
[assembly: SupportedOSPlatform("macOS")]
#elif LINUX
[assembly: SupportedOSPlatform("Linux")]
#elif WINDOWS_DESKTOP_BRIDGE
//using Microsoft.Toolkit.Uwp.Notifications;
#if DEBUG
using WinFormsMessageBox = System.Windows.Forms.MessageBox;
#endif
[assembly: SupportedOSPlatform("Windows10.0.17763.0")]
#elif WINDOWS
[assembly: SupportedOSPlatform("Windows7.0")]
#endif
#pragma warning restore SA1516 // Elements should be separated by blank line

namespace System.Application.UI
{
    static partial class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            }
            catch (NotSupportedException)
            {

            }

#if WINDOWS_DESKTOP_BRIDGE
#if DEBUG
            WinFormsMessageBox.Show(string.Join(' ', args), "Main(string[] args)");
#endif
            if (!DesktopBridgeHelper.Init()) return 0;

            //if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
            //{

            //    // 通过通知中心点击通知启动的进程
            //    args = Array.Empty<string>();
            //    //Handle when activated by click on notification
            //    ToastNotificationManagerCompat.OnActivated += toastArgs =>
            //    {
            //        //Get the activation args, if you need those.
            //        ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
            //        //Get user input if there's any and if you need those.
            //        var userInput = toastArgs.UserInput;
            //        //if the app instance just started after clicking on a notification 
            //        if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
            //        {
            //            System.Windows.Forms.MessageBox.Show("App was not running, " +
            //                   "but started and activated by click on a notification.");
            //        }
            //        else
            //        {
            //            System.Windows.Forms.MessageBox.Show("App was running, " +
            //                "and activated by click on a notification.");
            //        }
            //    };

            //    Console.ReadLine();
            //    while (true)
            //    {

            //    }
            //}

            DesktopBridgeHelper.OnActivated(ref args);
#elif !__MOBILE__
#if MAC
            AppDelegate.Init(/*args*/);
            FileSystemDesktopMac.InitFileSystem();
#elif LINUX
            FileSystemDesktopXDG.InitFileSystem();
#elif WINDOWS
            FileSystemDesktopWindows.InitFileSystem();
#else
            FileSystem2.InitFileSystem();
#endif
#endif
#if StartupTrace
            StartupTrace.Restart();
#endif
            // 目前桌面端默认使用 SystemTextJson 如果出现兼容性问题可取消下面这行代码
            // Serializable.DefaultJsonImplType = Serializable.JsonImplType.NewtonsoftJson;
            IsMainProcess = args.Length == 0;
            IsCLTProcess = !IsMainProcess && args.FirstOrDefault() == "-clt";

            IApplication.InitLogDir();
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