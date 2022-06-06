using System.Net;
using System.Application.CommandLine;
#if WINDOWS
using System.Runtime.InteropServices;
using WinRT;
using Microsoft.UI.Dispatching;
using WinUIApp = System.Application.UI.WinUI.App;
using WinUIApplication = Microsoft.UI.Xaml.Application;
#elif IOS || MACCATALYST
using UIKit;
using Foundation;
#elif ANDROID
using Android.App;
using Android.Runtime;
using Android.Content.PM;
#endif

namespace System.Application.UI;

public static partial class MauiProgram
{
    const string command_main = CommandLineHost.command_main;

    static MauiAppBuilder? builder;

    static string[] GetCommandLineArgs()
    {
        try
        {
            var args = Environment.GetCommandLineArgs();
            return args.ThrowIsNull();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    internal static MauiApp CreateMauiApp(string[]? args = null)
    {
        if (builder == null) Main(args ?? GetCommandLineArgs());
        var app = builder.ThrowIsNull().Build();
        DI.ConfigureServices(app.Services);
        OnBuild();
        return app;
    }

#if WINDOWS
    [STAThread]
#endif
    static int Main(string[] args)
    {
#if WINDOWS
        // fix The request was aborted: Could not create SSL/TLS secure channel
        TrySetSecurityProtocol();
#endif

        var host = ProgramHost.Instance;

        host.ConfigureServicesDelegate = level =>
        {
            builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp(_ => new App(host))
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            ConfigureServices(host, level, builder.Services);
        };

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

    static void StartMauiApp(string[] args)
    {
#if WINDOWS
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
#elif IOS || MACCATALYST
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
#elif TIZEN
        var app = new TizenApp_();
        app.Run(args);
#endif
    }

#if WINDOWS
    [DllImport("Microsoft.ui.xaml.dll")]
    static extern void XamlCheckProcessRequirements();
#endif
}

#if IOS || MACCATALYST
[Register("AppDelegate")]
public sealed partial class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
#elif ANDROID
[Application]
public sealed class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public sealed class MainActivity : MauiAppCompatActivity
{
}
#elif TIZEN
class TizenApp_ : MauiApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
#endif