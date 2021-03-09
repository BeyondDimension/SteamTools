using Avalonia;
using Avalonia.ReactiveUI;
using NLog;
using System.Properties;
using System.Reflection;
using System.Security;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using System.IO;

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
                Migrations.FromV1();
                Startup.Init();

#if DEBUG
                //TestSecurityStorage();
#endif

#if WINDOWS
                var app = new WpfApp();
                app.InitializeComponent();
                Task.Factory.StartNew(app.Run);
#endif

                CefNetApp.Init(args);

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                AppHelper.TrySetLoggerMinLevel(LogLevel.Trace);
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
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
               .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
               .With(new Win32PlatformOptions { AllowEglInitialization = true })
               .LogToTrace()
               .UseReactiveUI();

        static void FixNLogConfig()
        {
            var isMac = DI.Platform == Platform.Apple && DI.DeviceIdiom == DeviceIdiom.Desktop;
            const char directorySeparatorChar = '\\';
            if (isMac || Path.DirectorySeparatorChar != directorySeparatorChar)
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
                    string newValue;
                    if (isMac)
                    {
                        newValue = $"~/Library/Logs/{BuildConfig.APPLICATION_ID}/";
                    }
                    else
                    {
                        newValue = $"\"Logs{Path.DirectorySeparatorChar}";
                    }
                    logConfigStr = logConfigStr.Replace("\"Logs\\", newValue, StringComparison.OrdinalIgnoreCase);
                    File.Delete(filePath);
                    File.WriteAllText(filePath, logConfigStr);
                }
            }
        }

#if DEBUG

        //static async void TestSecurityStorage()
        //{
        //    await IStorage.Instance.SetAsync("↑↑", Encoding.UTF8.GetBytes("↓↓"));

        //    var left_top = Encoding.UTF8.GetString((
        //        await IStorage.Instance.GetAsync<byte[]>("↑↑")).ThrowIsNull("↑-key"));

        //    if (left_top != "↓↓")
        //    {
        //        throw new Exception();
        //    }

        //    await IStorage.Instance.SetAsync<string>("←←", "→→");

        //    var left_left = await IStorage.Instance.GetAsync<string>("←←");

        //    if (left_left != "→→")
        //    {
        //        throw new Exception();
        //    }

        //    await IStorage.Instance.SetAsync("aa", "bb");

        //    var left_aa = await IStorage.Instance.GetAsync("aa");

        //    if (left_aa != "bb")
        //    {
        //        throw new Exception();
        //    }

        //    var dict = new Dictionary<string, string> {
        //        { "🎈✨", "🎆🎇" },
        //        { "✨🎊", "🎃🎑" },
        //    };

        //    await IStorage.Instance.SetAsync("dict", dict);

        //    var left_dict = await IStorage.Instance.GetAsync<Dictionary<string, string>>("dict");

        //    if (left_dict == null)
        //    {
        //        throw new Exception();
        //    }

        //    if (left_dict.Count != dict.Count)
        //    {
        //        throw new Exception();
        //    }
        //}

#endif
    }
}