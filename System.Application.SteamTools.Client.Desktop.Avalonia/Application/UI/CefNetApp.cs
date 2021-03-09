using Avalonia.Threading;
using CefNet;
using System.IO;
using System.Linq;
using System.Properties;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.UI
{
    public sealed class CefNetApp : CefNetApplication
    {
        const string TAG = nameof(CefNetApp);

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            base.OnBeforeCommandLineProcessing(processType, commandLine);
#if DEBUG
            Log.Debug(TAG, "ChromiumWebBrowser_OnBeforeCommandLineProcessing");
            Log.Debug(TAG, commandLine.CommandLineString);
#endif

            //commandLine.AppendSwitchWithValue("proxy-server", "127.0.0.1:8888");

            //commandLine.AppendSwitchWithValue("remote-debugging-port", "9222");
            commandLine.AppendSwitch("off-screen-rendering-enabled");
            commandLine.AppendSwitchWithValue("off-screen-frame-rate", "30");

            if (ThisAssembly.Debuggable)
            {
                //enable-devtools-experiments
                commandLine.AppendSwitch("enable-devtools-experiments");
            }

            //e.CommandLine.AppendSwitchWithValue("user-agent", "Mozilla/5.0 (Windows 10.0) WebKa/" + DateTime.UtcNow.Ticks);

            //("force-device-scale-factor", "1");

            commandLine.AppendSwitch("disable-gpu");
            commandLine.AppendSwitch("disable-gpu-compositing");
            commandLine.AppendSwitch("disable-gpu-vsync");

            commandLine.AppendSwitch("enable-begin-frame-scheduling");
            commandLine.AppendSwitch("enable-media-stream");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                commandLine.AppendSwitch("no-zygote");
                commandLine.AppendSwitch("no-sandbox");
            }
        }

        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            base.OnContextCreated(browser, frame, context);
            frame.ExecuteJavaScript(@"
{
const newProto = navigator.__proto__;
delete newProto.webdriver;
navigator.__proto__ = newProto;
}", frame.Url, 0);
        }

        public Action<long>? ScheduleMessagePumpWorkCallback { get; set; }

        protected override void OnScheduleMessagePumpWork(long delayMs)
        {
            ScheduleMessagePumpWorkCallback?.Invoke(delayMs);
        }

        public static CefNetAppInitState InitState { get; private set; }

        static CefNetApp? app;
        static Timer? messagePump;
        const int messagePumpDelay = 10;

        public static CefNetApp Current => app ?? throw new NullReferenceException("CefNetApp init must be called.");

        public static void Init(string[] args)
        {
            if (InitState != CefNetAppInitState.Uninitialized) return;

            var cefPath = DI.Platform switch
            {
                Platform.Windows => Path.Combine(AppContext.BaseDirectory, "CEF", "win-x86"),
                Platform.Linux => Path.Combine(AppContext.BaseDirectory, "CEF", "linux-x64"),
                Platform.Apple => DI.DeviceIdiom == DeviceIdiom.Desktop ? Path.Combine(AppContext.BaseDirectory, "Contents", "Frameworks", "Chromium Embedded Framework.framework") : throw new PlatformNotSupportedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(DI.Platform), DI.Platform, null),
            };

            if (!Directory.Exists(cefPath))
            {
                Log.Error(TAG, "Missing Chromium Embedded Framework Binaries, path: {0}", cefPath);
                InitState = CefNetAppInitState.MissingBinaries;
                return;
            }

            var localesDirPath = Path.Combine(cefPath, "locales");
            if (!Directory.Exists(localesDirPath))
            {
                Log.Error(TAG, "Missing Chromium Embedded Framework Binaries(locales), path: {0}", cefPath);
                InitState = CefNetAppInitState.MissingBinaries;
                return;
            }

            var externalMessagePump = args.Contains("--external-message-pump");

            if (PlatformInfo.IsMacOS)
            {
                externalMessagePump = true;
            }

            var logFile = DI.Platform switch
            {
                Platform.Windows or Platform.Linux => Path.Combine(AppContext.BaseDirectory, "Logs", "cef.log"),
                Platform.Apple => DI.DeviceIdiom == DeviceIdiom.Desktop ? $"~/Library/Logs/{BuildConfig.APPLICATION_ID}/cef.log" : throw new PlatformNotSupportedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(DI.Platform), DI.Platform, null),
            };

            var logDirPath = Path.GetDirectoryName(logFile);
            IOPath.DirCreateByNotExists(logDirPath);

            var settings = new CefSettings
            {
                MultiThreadedMessageLoop = !externalMessagePump,
                ExternalMessagePump = externalMessagePump,
                NoSandbox = true,
                WindowlessRenderingEnabled = true,
                LocalesDirPath = localesDirPath,
                ResourcesDirPath = cefPath,
                LogSeverity = ThisAssembly.Debuggable ? CefLogSeverity.Warning : CefLogSeverity.Error,
                LogFile = logFile,
                IgnoreCertificateErrors = true,
                UncaughtExceptionStackSize = 8,
            };

            AppHelper.Initialized += () =>
                {
                    if (PlatformInfo.IsMacOS || Environment.GetCommandLineArgs().Contains("--external-message-pump"))
                    {
                        messagePump = new Timer(_ => Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork), null, messagePumpDelay, messagePumpDelay);
                    }
                };
            AppHelper.Shutdown += () =>
            {
                messagePump?.Dispose();
                app?.Shutdown();
            };

            app = new CefNetApp
            {
                ScheduleMessagePumpWorkCallback = async delayMs =>
                {
                    await Task.Delay((int)delayMs);
                    Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork);
                }
            };
            app.Initialize(cefPath, settings);

            InitState = CefNetAppInitState.Complete;
        }
    }

    public enum CefNetAppInitState
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        Uninitialized,

        /// <summary>
        /// 丢失二进制文件
        /// </summary>
        MissingBinaries,

        /// <summary>
        /// 初始化完成
        /// </summary>
        Complete,
    }
}