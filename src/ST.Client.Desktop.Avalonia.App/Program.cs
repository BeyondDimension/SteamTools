using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Versioning;
using System.Application.Settings;
using System.Application.CommandLine;
using NLog;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Controls;

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
        const string command_main = CommandLineHost.command_main;

        [STAThread]
        static int Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // fix The request was aborted: Could not create SSL/TLS secure channel
            TrySetSecurityProtocol();

            var host = ProgramHost.Instance;

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
                return host.Run(args_clt);
            }
            catch (Exception ex)
            {
                GlobalExceptionHelpers.Handler(ex, nameof(Main));
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                PlatformApp?.Dispose();
                DI.Dispose();
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
            var builder = AppBuilder.Configure(() => new App(ProgramHost.Instance))
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
    }
}