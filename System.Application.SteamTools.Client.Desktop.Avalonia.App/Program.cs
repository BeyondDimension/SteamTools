using Avalonia;
using Avalonia.ReactiveUI;
using NLog;
using NLog.Config;
using System.IO;
using System.Properties;
using System.Reflection;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
            // 目前桌面端默认使用 SystemTextJson 如果出现兼容性问题可取消下面这行代码
            // Serializable.DefaultJsonImplType = Serializable.JsonImplType.NewtonsoftJson;

            var isMainProcess = args.Length == 0;
            var logDirPath = InitLogDir();

            void InitCefNetApp() => CefNetApp.Init(logDirPath, args);

            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                if (isMainProcess)
                {
                    Migrations.FromV1();
                }

                Startup.Init(isMainProcess);

                if (ThisAssembly.Debuggable)
                {
                    Log.Warn(nameof(Program),
                        "Main isMainProcess: {0}, args={1}",
                        isMainProcess,
                        string.Join(' ', args));
                }

#if WINDOWS
                //var app = new WpfApp();
                //app.InitializeComponent();
                //Task.Factory.StartNew(app.Run);
#endif

                InitCefNetApp();

                if (isMainProcess)
                {
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }
            }
            catch (Exception ex)
            {
                AppHelper.TrySetLoggerMinLevel(LogLevel.Trace);
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");

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
                            logger.Error(ex_shutdown_app_helper,
                                "(AppHelper)Close exception when exception occurs");
                        }
                    }
                }
                catch (Exception ex_shutdown)
                {
                    logger.Error(ex_shutdown,
                        "(App)Close exception when exception occurs");
                }

#if DEBUG
                //MessageBoxCompat.Show(ex.ToString(), "Steam++ Run Error" + ThisAssembly.Version, MessageBoxButtonCompat.OK, MessageBoxImageCompat.Warning);
#endif
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
               .With(new SkiaOptions
               {
                   MaxGpuResourceSizeBytes = 8096000,
               })
               .With(new Win32PlatformOptions
               {
                   AllowEglInitialization = true,
               })
               .LogToTrace()
               .UseReactiveUI();

        static string InitLogDir()
        {
            var logDirPath = Path.Combine(AppContext.BaseDirectory, "Logs");
            IOPath.DirCreateByNotExists(logDirPath);

            var logDirPath_ = logDirPath + Path.DirectorySeparatorChar;

            var xmlConfigStr =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                "<nlog xmlns=\"http://www.nlog-project.org/schemas/NLog.xsd\"" +
                "      xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                "      autoReload=\"true\"" +
                $"      internalLogFile=\"" + logDirPath_ + "internal-nlog.txt\"" +
                "      internalLogLevel=\"Off\">" +
                "  <targets>" +
                "    <target xsi:type=\"File\" name=\"logfile\" fileName=\"" + logDirPath_ + "nlog-all-${shortdate}.log\"" +
                "            layout=\"${longdate}|${level}|${message} |${all-event-properties} ${exception:format=tostring}\" />" +
                "    <target xsi:type=\"Console\" name=\"logconsole\"" +
                "            layout=\"${longdate}|${level}|${message} |${all-event-properties} ${exception:format=tostring}\" />" +
                "  </targets>" +
                "  <rules>" +
                "    <logger name=\"*\" minlevel=\"" + AppHelper.DefaultNLoggerMinLevel.Name + "\" writeTo=\"logfile,logconsole\"/>" +
                "  </rules>" +
                "</nlog>"
            ;

            var xmlConfig = XmlLoggingConfiguration.CreateFromXmlString(xmlConfigStr);
            LogManager.Configuration = xmlConfig;

            return logDirPath;
        }
    }
}