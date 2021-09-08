using Avalonia.Threading;
using CefNet;
using CefNet.Input;
using System;
using System.Application.Models;
using System.Globalization;
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
            //Log.Debug(TAG, "ChromiumWebBrowser_OnBeforeCommandLineProcessing");
            //Log.Debug(TAG, commandLine.CommandLineString);
#endif

            //commandLine.AppendSwitchWithValue("proxy-server", "127.0.0.1:8888");

            //commandLine.AppendSwitchWithValue("remote-debugging-port", "9222");
            //commandLine.AppendSwitch("off-screen-rendering-enabled");
            //commandLine.AppendSwitchWithValue("off-screen-frame-rate", "30");

            //if (ThisAssembly.Debuggable)
            //{
            //    //enable-devtools-experiments
            //    commandLine.AppendSwitch("enable-devtools-experiments");
            //}

            //e.CommandLine.AppendSwitchWithValue("user-agent", "Mozilla/5.0 (Windows 10.0) WebKa/" + DateTime.UtcNow.Ticks);

            //("force-device-scale-factor", "1");

            commandLine.AppendSwitchWithValue("disable-gpu", "1");
            commandLine.AppendSwitchWithValue("disable-gpu-compositing", "1");
            commandLine.AppendSwitchWithValue("disable-gpu-vsync", "1");
            commandLine.AppendSwitchWithValue("disable-gpu-shader-disk-cache", "1");

            //commandLine.AppendSwitch("enable-begin-frame-scheduling");
            //commandLine.AppendSwitch("enable-media-stream");

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //{
            //    commandLine.AppendSwitch("no-zygote");
            //    commandLine.AppendSwitch("no-sandbox");
            //}

            //commandLine.AppendSwitch("disable-web-security"); // LoginUsingSteamClient
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

        static CefNetAppInitState _InitState;
        public static CefNetAppInitState InitState
        {
            get => _InitState;
            private set
            {
                _InitState = value;
                AppHelper.IsSystemWebViewAvailable = value == CefNetAppInitState.Complete;
            }
        }

        static CefNetApp? app;
        static Timer? messagePump;
        const int messagePumpDelay = 10;

        public static CefNetApp Current => app ?? throw new NullReferenceException("CefNetApp init must be called.");

        public static void Init(string logDirPath, string[] args)
        {
            if (InitState != CefNetAppInitState.Uninitialized) return;

            static string GetArch() => RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "x86",
                Architecture.X64 => "x64",
                Architecture.Arm64 => "arm64",
                _ => throw new PlatformNotSupportedException(),
            };

            var cefPath = DI.Platform switch
            {
                Platform.Windows => Path.Combine(AppContext.BaseDirectory, "CEF", "win-" + GetArch()),
                Platform.Linux => Path.Combine(AppContext.BaseDirectory, "CEF", "linux-" + GetArch()),
                Platform.Apple => DI.DeviceIdiom == DeviceIdiom.Desktop ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Contents", "Frameworks", "Chromium Embedded Framework.framework")) : throw new PlatformNotSupportedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(DI.Platform), DI.Platform, null),
            };

#if DEBUG
            if (BuildConfig.IsAigioPC)
            {
                cefPath = @"G:\CEF\win-" + GetArch();
            }
#endif

            if (!Directory.Exists(cefPath))
            {
                Log.Error(TAG, "Missing Chromium Embedded Framework Binaries, path: {0}", cefPath);
                InitState = CefNetAppInitState.MissingBinaries;
                return;
            }

            string? localesDirPath = null;
            if (!PlatformInfo.IsMacOS)
            {
                localesDirPath = Path.Combine(cefPath, "locales");
                if (!Directory.Exists(localesDirPath))
                {
                    Log.Error(TAG, "Missing Chromium Embedded Framework Binaries(locales), path: {0}", cefPath);
                    InitState = CefNetAppInitState.MissingBinaries;
                    return;
                }
            }

            var externalMessagePump = args.Contains("--external-message-pump");

            if (PlatformInfo.IsMacOS)
            {
                externalMessagePump = true;
            }

            var logFile = Path.Combine(logDirPath, "cef.log");

            var settings = new CefSettings
            {
                Locale = "zh-CN",
                AcceptLanguageList = CultureInfo.CurrentUICulture.GetAcceptLanguage(),
                MultiThreadedMessageLoop = !externalMessagePump,
                ExternalMessagePump = externalMessagePump,
                NoSandbox = true,
                WindowlessRenderingEnabled = true,
                // https://magpcss.org/ceforum/viewtopic.php?t=14648#p32857
                LogSeverity = ThisAssembly.Debuggable ? CefLogSeverity.Error : CefLogSeverity.Disable,
                LogFile = logFile,
                IgnoreCertificateErrors = true,
                UncaughtExceptionStackSize = 8,
            };

            if (PlatformInfo.IsMacOS)
            {
                var resourcesDirPath = Path.Combine(cefPath, "Resources");
                if (!Directory.Exists(resourcesDirPath))
                {
                    Log.Error(TAG, "Missing Chromium Embedded Framework Resources, path: {0}", resourcesDirPath);
                    InitState = CefNetAppInitState.MissingBinaries;
                    return;
                }
                settings.ResourcesDirPath = resourcesDirPath;
                settings.NoSandbox = true;
            }
            else
            {
                settings.LocalesDirPath = localesDirPath;
                settings.ResourcesDirPath = cefPath;
            }

            AppHelper.Initialized += () =>
            {
                if (Instance.UsesExternalMessageLoop)
                {
                    messagePump = new Timer(_ => Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork), null, messagePumpDelay, messagePumpDelay);
                }
            };

            AppHelper.Shutdown += () =>
            {
                try
                {
                    messagePump?.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error("Shutdown", ex, "messagePump?.Dispose()");
                }
                try
                {
                    app?.Shutdown();
                }
                catch (Exception ex)
                {
                    Log.Error("Shutdown", ex, "app?.Shutdown()");
                }
            };

            KeycodeConverter.Default = new FixChineseInptKeycodeConverter();

            app = new CefNetApp
            {
                ScheduleMessagePumpWorkCallback = async delayMs =>
                {
                    await Task.Delay((int)delayMs);
                    Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork);
                }
            };
            // https://bitbucket.org/chromiumembedded/cef/wiki/JavaScriptIntegration.md
            //app.WebKitInitialized += (_, _) =>
            //{
            //    try
            //    {
            //        // 疑似渲染进程中执行，取不到值。
            //        var extensionCode =
            //        "var steampp;" +
            //        "if (!steampp)" +
            //        "  steampp = {};" +
            //        "(function() {" +
            //       $"  steampp.language = \"{R.Language}\";" +
            //       $"  steampp.theme = \"{GetTheme()}\";" +
            //        "})();";
            //        CefApi.RegisterExtension("steampp.js", extensionCode, null);
            //    }
            //    catch (Exception e)
            //    {
            //        Log.Error(nameof(WebKitInitialized), e, "Register steampp.js Fail.");
            //    }
            //};
            app.Initialize(cefPath, settings);

            InitState = CefNetAppInitState.Complete;
        }

        public static string GetTheme()
        {
            var theme = AppHelper.Current.Theme;
            return theme switch
            {
                AppTheme.FollowingSystem => GetThemeStringByFollowingSystem(),
                _ => theme.ToString2(),
            };
            static string GetThemeStringByFollowingSystem()
            {
                if (DI.Platform == Platform.Windows)
                {
                    var major = Environment.OSVersion.Version.Major;
                    if (major < 10 || major == 10 && Environment.OSVersion.Version.Build < 18282)
                    {
                        goto dark;
                    }
                }
                else if (DI.Platform == Platform.Linux)
                {
                    goto dark;
                }
                return AppTheme.FollowingSystem.ToString2();
            dark: return AppTheme.Dark.ToString2();
            }
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

namespace CefNet.Input
{
    /// <summary>
    /// https://github.com/CefNet/CefNet/issues/21
    /// </summary>
    public class FixChineseInptKeycodeConverter : KeycodeConverter
    {
        public override VirtualKeys CharacterToVirtualKey(char character)
        {
            // https://github.com/CefNet/CefNet/blob/master/CefNet/Input/KeycodeConverter.cs#L41
            // https://github.com/CefNet/CefNet/blob/90.5.21109.1453/CefNet/Input/KeycodeConverter.cs#L41
            try
            {
                return base.CharacterToVirtualKey(character);
            }
            catch (Exception e)
            {
                if (PlatformInfo.IsWindows)
                {
                    if (e.Message == "Incompatible input locale.")
                    {
                        return VirtualKeys.None;
                    }
                }
                throw;
            }
        }
    }
}