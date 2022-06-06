using System.Net;
using System.Runtime.InteropServices;
using System.Application.CommandLine;
using WinRT;
using Microsoft.UI.Dispatching;
using WinUIApp = System.Application.UI.WinUI.App;
using WinUIApplication = Microsoft.UI.Xaml.Application;

namespace System.Application.UI
{
    public static partial class MauiProgram
    {
        const string command_main = CommandLineHost.command_main;

        static event Action? Exit;

        [DllImport("Microsoft.ui.xaml.dll")]
        static extern void XamlCheckProcessRequirements();

        [STAThread]
        static int Main(string[] args)
        {
            // fix The request was aborted: Could not create SSL/TLS secure channel
            TrySetSecurityProtocol();

            var host = ProgramHost.Instance;

            host.IsMainProcess = args.Length == 0;
            host.IsCLTProcess = !host.IsMainProcess && args.FirstOrDefault() == "-clt";

#if WINDOWS_DESKTOP_BRIDGE
            if (!DesktopBridgeHelper.Init()) return 0;
            InitWithUWP(args);
#endif
            OnCreateAppExecuting();

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
                try
                {
                    host.Application?.Dispose();
                }
                catch
                {

                }
                try
                {
                    host.AppInstance?.Dispose();
                }
                catch
                {

                }
                try
                {
                    Exit?.Invoke();
                }
                catch
                {

                }
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

        static void StartMauiApp()
        {
            // 不能以管理员权限启动 程序“.exe”已退出，返回值为 3221226505 (0xc0000409)。
            try
            {
                XamlCheckProcessRequirements();
            }
            catch (DllNotFoundException ex)
            {
                // 需要安装 Windows App Runtime
                // <WindowsPackageType>None</WindowsPackageType>
                throw new ApplicationException("Requires Windows App Runtime to be installed.", ex);
            }
            ComWrappersSupport.InitializeComWrappers();
            WinUIApplication.Start(p =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);

                var app = new WinUIApp();
            });
        }
    }
}
