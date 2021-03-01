using Avalonia;
using Avalonia.ReactiveUI;
using NLog;
using System.IO;
using System.Properties;
using System.Reflection;
using System.Threading.Tasks;

[assembly: AssemblyTitle(ThisAssembly.AssemblyTrademark + " v" + ThisAssembly.Version)]
namespace System.Application.UI
{
    static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        static void Main(string[] args)
        {
            FixNLogConfig();

            // 目前桌面端默认使用 SystemTextJson 如果出现兼容性问题可取消下面这行代码
            // Serializable.DefaultJsonImplType = Serializable.JsonImplType.NewtonsoftJson;

            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                Startup.Init();
#if WINDOWS
                var app = new WpfApp();
                app.InitializeComponent();
                Task.Factory.StartNew(app.Run);
#endif
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                AppHelper.SetNLoggerMinLevel(LogLevel.Trace);
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
           => AppBuilder.Configure<App>()
               .UsePlatformDetect()
               .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
               .With(new Win32PlatformOptions { AllowEglInitialization = true })
               .LogToTrace()
               .UseReactiveUI();

        static void FixNLogConfig()
        {
            const char directorySeparatorChar = '\\';
            if (Path.DirectorySeparatorChar != directorySeparatorChar)
            {
                const string fileName = "nlog.config";
                var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(filePath))
                {
                    string logConfigStr;
                    try
                    {
                        logConfigStr = File.ReadAllText(filePath);
                    }
                    catch
                    {
                        return;
                    }
                    logConfigStr = logConfigStr.Replace(@"logs\", $"logs{Path.DirectorySeparatorChar}");
                    File.Delete(filePath);
                    File.WriteAllText(filePath, logConfigStr);
                }
            }
        }
    }
}