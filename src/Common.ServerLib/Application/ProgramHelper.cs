using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using NLog;
using NLog.Config;
using NLog.Web;
using NLog.Targets;
using NInternalLogger = NLog.Common.InternalLogger;
using NLogLevel = NLog.LogLevel;
using NLogManager = NLog.LogManager;

namespace System.Application;

public static class ProgramHelper
{
    public static string Version { get; private set; } = string.Empty;

#if DEBUG
    [Obsolete("应迁移到 ASP.NET Core 6.0 中新的最小托管模型的代码", true)]
    public static void Main<TStartup>(string projectName,
        string[] args, Action<string>? writeLine = null) where TStartup : class
    {
        var logger = NLogBuilder.ConfigureNLog(InitNLogConfig()).GetCurrentClassLogger();
        try
        {
            logger.Debug("init main");
            var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                //logging.AddAzureWebAppDiagnostics(); // 添加 Azure Web App 日志 。
            })
            .UseNLog()  // NLog: Setup NLog for Dependency injection
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();
            })
            .Build();
            DI.ConfigureServices(host.Services);

            writeLine ??= Console.WriteLine;
            Console.OutputEncoding = Encoding.Unicode; // 使用 UTF16 编码输出控制台文字
            Version = typeof(TStartup).Assembly.GetName().Version?.ToString() ?? string.Empty;

            UnityConsoleOutputHead.Write(writeLine,
                projectName,
                Version,
                RuntimeVersion,
                CentralProcessorName);

            host.Run();
        }
        catch (Exception exception)
        {
            //NLog: catch setup errors
            logger.Error(exception, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            NLogManager.Shutdown();
        }
    }
#endif

    [Description("适用于 ASP.NET Core 6.0 中新的最小托管模型的代码")]
    public static void Main(
        string projectName,
        string[] args,
        Action<WebApplicationBuilder>? configureServices = null,
        Action<WebApplication>? configure = null)
    {
        var logger = NLogBuilder.ConfigureNLog(InitNLogConfig()).GetCurrentClassLogger();
        try
        {
            // https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-6
            logger.Debug("init main");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseNLog();
            configureServices?.Invoke(builder);
            var app = builder.Build();
            configure?.Invoke(app);
            DI.ConfigureServices(app.Services);

            Console.OutputEncoding = Encoding.Unicode; // 使用 UTF16 编码输出控制台文字
            Version = ((configureServices?.Method ?? configure?.Method)?.Module.Assembly ?? Assembly.GetCallingAssembly()).GetName().Version?.ToString() ?? string.Empty;

            UnityConsoleOutputHead.Write(Console.WriteLine,
                projectName,
                Version,
                RuntimeVersion,
                CentralProcessorName);

            app.Run();
        }
        catch (Exception exception)
        {
            //NLog: catch setup errors
            logger.Error(exception, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            NLogManager.Shutdown();
        }
    }

    static LoggingConfiguration InitNLogConfig()
    {
        // https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-6

        NInternalLogger.LogFile = $"logs{Path.DirectorySeparatorChar}internal-nlog.txt";
        NInternalLogger.LogLevel =
#if DEBUG
            NLogLevel.Info;
#else
            NLogLevel.Error;
#endif
        // enable asp.net core layout renderers
        NLogManager.Setup().SetupExtensions(s => s.RegisterAssembly("NLog.Web.AspNetCore"));

        var objConfig = new LoggingConfiguration();
        // File Target for all log messages with basic details
        var allfile = new FileTarget("allfile")
        {
            FileName = $"logs{Path.DirectorySeparatorChar}nlog-all-${{shortdate}}.log",
            Layout = "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}",
            ArchiveAboveSize = 10485760,
            MaxArchiveFiles = 15,
            MaxArchiveDays = 15,
        };
        objConfig.AddTarget(allfile);
        // File Target for own log messages with extra web details using some ASP.NET core renderers
        var ownFile_web = new FileTarget("ownFile-web")
        {
            FileName = $"logs{Path.DirectorySeparatorChar}nlog-own-${{shortdate}}.log",
            Layout = "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}",
            ArchiveAboveSize = 10485760,
            MaxArchiveFiles = 15,
            MaxArchiveDays = 15,
        };
        objConfig.AddTarget(ownFile_web);
        // Console Target for hosting lifetime messages to improve Docker / Visual Studio startup detection
        var lifetimeConsole = new ConsoleTarget("lifetimeConsole")
        {
            Layout = "${level:truncate=4:tolower=true}\\: ${logger}[0]${newline}      ${message}${exception:format=tostring}",
        };
        objConfig.AddTarget(lifetimeConsole);

        // All logs, including from Microsoft
        objConfig.AddRule(
#if DEBUG
            NLogLevel.Trace
#else
            NLogLevel.Error
#endif
            , NLogLevel.Fatal, allfile, "*");
        objConfig.AddRule(
#if DEBUG
            NLogLevel.Trace
#else
            NLogLevel.Error
#endif
            , NLogLevel.Fatal, ownFile_web, "*");

        foreach (var target in objConfig.AllTargets)
        {
            // Skip non-critical Microsoft logs and so log only own logs (BlackHole)
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, target, "Microsoft.*", true);
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, target, "System.Net.Http.*", true);
        }

        foreach (var target in new Target[] { lifetimeConsole, ownFile_web })
        {
            // Output hosting lifetime messages to console target for faster startup detection
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, target, "Microsoft.Hosting.Lifetime", true);
        }

        return objConfig;

        //        var xmlConfigStr =
        //          "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
        //          "<nlog xmlns=\"http://www.nlog-project.org/schemas/NLog.xsd\"" +
        //          "      xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
        //          "      autoReload=\"true\"" +
        //          "      internalLogLevel=\"" +
        //#if DEBUG
        //          "Info"
        //#else
        //          "Error"
        //#endif
        //          + "\"" +
        //          "      internalLogFile=\"logs" + Path.DirectorySeparatorChar + "internal-nlog.txt\">" +
        //          // enable asp.net core layout renderers
        //          "  <extensions>" +
        //          "    <add assembly=\"NLog.Web.AspNetCore\"/>" +
        //          "  </extensions>" +
        //          // the targets to write to
        //          "  <targets>" +
        //          // write logs to file
        //          "    <target xsi:type=\"File\" name=\"allfile\" fileName=\"logs" + Path.DirectorySeparatorChar + "nlog-all-${shortdate}.log\"" +
        //          "            layout=\"${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}\"/>" +
        //          // another file log, only own logs. Uses some ASP.NET core renderers
        //          "    <target xsi:type=\"File\" name=\"ownFile-web\" fileName=\"logs" + Path.DirectorySeparatorChar + "nlog-own-${shortdate}.log\"" +
        //          "            layout=\"${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}\"/>" +
        //          // Console Target for hosting lifetime messages to improve Docker / Visual Studio startup detection
        //          "    <target xsi:type=\"Console\" name=\"lifetimeConsole\" layout=\"${level:truncate=4:tolower=true}\\: ${logger}[0]${newline}      ${message}${exception:format=tostring}\" />" +
        //          "  </targets>" +
        //          // rules to map from logger name to target
        //          "  <rules>" +
        //          // All logs, including from Microsoft
        //          "    <logger name=\"*\" minlevel=\"" +
        //#if DEBUG
        //          "Trace"
        //#else
        //          "Error"
        //#endif
        //          + "\" writeTo=\"allfile\"/>" +
        //          // Output hosting lifetime messages to console target for faster startup detection
        //          "    <logger name=\"Microsoft.Hosting.Lifetime\" minlevel=\"Info\" writeTo=\"lifetimeConsole, ownFile-web\" final=\"true\" />" +
        //          // Skip non-critical Microsoft logs and so log only own logs
        //          "    <logger name=\"Microsoft.*\" maxLevel=\"Info\" final=\"true\"/>" +
        //          "    <logger name=\"System.Net.Http.*\" maxlevel=\"Info\" final=\"true\" />" +
        //          // BlackHole without writeTo
        //          "    <logger name=\"*\" minlevel=\"" +
        //#if DEBUG
        //          "Trace"
        //#else
        //          "Error"
        //#endif
        //          + "\" writeTo=\"ownFile-web\"/>" +
        //          "  </rules>" +
        //          "</nlog>";

        //        var xmlConfig = XmlLoggingConfiguration.CreateFromXmlString(xmlConfigStr);

        //        return xmlConfig;
    }

    //static readonly Lazy<string> mRuntimeVersion = new(() =>
    //{
    //    try
    //    {
    //        var process = Process.GetCurrentProcess();
    //        var module = process.Modules.Cast<ProcessModule>().FirstOrDefault(x
    //            => x != null && x.ModuleName != null &&
    //            (x.ModuleName.StartsWith("hostfxr", StringComparison.OrdinalIgnoreCase) ||
    //            x.ModuleName.StartsWith("hostpolicy", StringComparison.OrdinalIgnoreCase)
    //        ));
    //        if (module != null)
    //        {
    //            return module.FileVersionInfo.FileDescription?.Split(" - ").LastOrDefault()?.Trim() ?? string.Empty;
    //        }
    //    }
    //    catch
    //    {
    //    }
    //    return string.Empty;
    //});

    /// <summary>
    /// 获取当前运行时版本
    /// </summary>
    public static string RuntimeVersion =>
        Environment.Version.ToString();
    //mRuntimeVersion.Value;

    static readonly Lazy<string> mCentralProcessorName = new(() =>
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                RegistryKey? registryKey = null;
                try
                {
                    registryKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0\");
                    return registryKey?.GetValue("ProcessorNameString")?.ToString()?.Trim() ?? string.Empty;
                }
                catch
                {
                }
                finally
                {
                    registryKey?.Dispose();
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                const string filePath = $"{IOPath.UnixDirectorySeparatorCharAsString}proc{IOPath.UnixDirectorySeparatorCharAsString}cpuinfo";
                if (File.Exists(filePath))
                {
                    using var fs = File.OpenRead(filePath);
                    using var sr = new StreamReader(fs);
                    while (sr.Peek() >= 0)
                    {
                        var line = sr.ReadLine();
                        if (line == null) break;
                        var array = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        if (array.Length == 2)
                        {
                            if (array[0].Trim() == "model name")
                            {
                                return array[1].Trim();
                            }
                        }
                    }
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                return Process2.RunShell(Process2.BinBash, "sysctl -n machdep.cpu.brand_string");
            }
        }
        catch
        {

        }
        return string.Empty;
    });

    /// <summary>
    /// 获取当前运行的计算机CPU显示名称，仅支持 Windows OS
    /// </summary>
    public static string CentralProcessorName => mCentralProcessorName.Value;

    public static IActionResult GetInfo(ControllerBase controller, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            var now = DateTime.Now;
            return new JsonResult(new
            {
                Version,
                RuntimeVersion,
                CentralProcessorName = $"{CentralProcessorName} x{Environment.ProcessorCount}",
                environment.WebRootPath,
                environment.ContentRootPath,
                environment.EnvironmentName,
                environment.ApplicationName,
                CurrentCulture = CultureInfo.CurrentCulture.EnglishName,
                RawUrl = controller.Request.RawUrl(),
                controller.Request.Protocol,
                AcceptWebP = controller.Request.AcceptWebP(),
                UserHostAddress = controller.Request.UserHostAddress(),
                UserAgent = controller.Request.UserAgent(),
                AcceptLanguage = controller.Request.Headers["Accept-Language"].ToString(),
                //AcceptLanguage2 = controller.Request.Headers["Accept-Language2"].ToString(),
                Now = new Dictionary<string, object>
                {
                    { "Default", now },
                    { "RFC1123", now.ToString("r") },
                    { "Standard", now.ToString(DateTimeFormat.Standard) },
                },
                DayOfWeek = now.DayOfWeek.ToString2(true),
                //Process = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Select(x => x.FileName).ToArray(),
            });
        }
        return controller.NotFound();
    }

    public static IActionResult GetEnv(ControllerBase controller, IWebHostEnvironment environment)
    {
        var content = $"<h1>{environment.EnvironmentName}</h1>";
        return controller.Content(content, MediaTypeNames.HTML);
    }

    /// <inheritdoc cref="Path2.InitFileSystem(Func{string}, Func{string})"/>
    public static void InitFileSystem(IWebHostEnvironment environment)
    {
        IOPath.InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
        string GetAppDataDirectory()
        {
            var pathAppData = Path.Combine(environment.ContentRootPath, "AppData");
            IOPath.DirCreateByNotExists(pathAppData);
            return pathAppData;
        }
        string GetCacheDirectory()
        {
            var pathCache = Path.Combine(environment.ContentRootPath, "Cache");
            IOPath.DirCreateByNotExists(pathCache);
            return pathCache;
        }
    }
}