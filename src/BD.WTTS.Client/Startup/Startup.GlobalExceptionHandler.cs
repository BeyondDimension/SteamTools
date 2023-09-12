using System.Reactive;
using Logger = NLog.Logger;
#if ANDROID || __ANDROID__
using JavaObject = Java.Lang.Object;
using JavaThread = Java.Lang.Thread;
using JavaThrowable = Java.Lang.Throwable;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // 全局异常处理
{
    /// <summary>
    /// 全局异常处理
    /// </summary>
    public static class GlobalExceptionHandler
    {
        const string TAG = nameof(GlobalExceptionHandler);

        static int index_exceptions;
        static readonly int[] exceptions = new int[3];
        static readonly object lock_global_ex_log = new();
        static readonly Lazy<Logger> _logger = new(LogManager.GetCurrentClassLogger);

        static Logger Logger => _logger.Value;

#if ANDROID || __ANDROID__
        sealed class UncaughtExceptionHandler : JavaObject, JavaThread.IUncaughtExceptionHandler
        {
            readonly Action<JavaThread, JavaThrowable> action;
            readonly JavaThread.IUncaughtExceptionHandler? @interface;

            public UncaughtExceptionHandler(Action<JavaThread, JavaThrowable> action, JavaThread.IUncaughtExceptionHandler? @interface = null)
            {
                this.action = action;
                this.@interface = @interface;
            }

            public void UncaughtException(JavaThread t, JavaThrowable e)
            {
                @interface?.UncaughtException(t, e);
                action(t, e);
            }
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Init()
        {
#if __ANDROID__
            JavaThread.DefaultUncaughtExceptionHandler = new UncaughtExceptionHandler((_, ex) =>
            {
                Handler(ex, nameof(Java));
            }, JavaThread.DefaultUncaughtExceptionHandler);
#endif
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    Handler(ex, nameof(AppDomain), e.IsTerminating);
                }
            };
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
            {
                // https://github.com/AvaloniaUI/Avalonia/issues/5290#issuecomment-760751036
                Handler(ex, nameof(RxApp));
            });
        }

        public static void Handler(Exception ex, string name, bool? isTerminating = null)
        {
            var hashCode = ex.GetHashCode();
            if (exceptions.Contains(hashCode))
                return;

            lock (lock_global_ex_log)
            {
                if (index_exceptions >= exceptions.Length)
                    index_exceptions = 0;
                exceptions[index_exceptions++] = hashCode;
            }

            // NLog: catch any exception and log it.
            var message = "Stopped program because of exception, name: {1}, isTerminating: {0}";
            var args = new object?[] { isTerminating, name, };
#if DEBUG
            Console.WriteLine(string.Format(message, args));
#endif
            Logger.Error(ex, message, args);

#if WINDOWS || LINUX || APP_REVERSE_PROXY
            try
            {
                VisualStudioAppCenterSDK.UtilsImpl.Instance.InvokeUnhandledExceptionOccurred?.Invoke(null, ex);
            }
            catch
            {

            }
#endif

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

            Startup? s;
            try
            {
                s = Instance;
            }
            catch
            {
                s = null;
            }

            if (s != null)
            {
                if (s.TryGetPlugins(out var plugins))
                {
                    foreach (var plugin in plugins)
                    {
                        try
                        {
                            plugin.OnUnhandledException(ex, name, isTerminating);
                        }
                        catch (Exception ex_plugin)
                        {
#if DEBUG
                            Console.WriteLine("(App)Close exception when OnUnhandledException");
                            Console.WriteLine(ex_plugin);
#endif
                            Logger.Error(ex_plugin,
                                "(App)Close exception when OnUnhandledException");
                        }
                    }
                }
            }
#endif

#if WINDOWS
            var mbText =
$"""
{string.Format(message, args)}
{ex}
""";
            ShowApplicationCrash(mbText);
#endif
        }

#if WINDOWS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static WPFMessageBoxResult? ShowApplicationCrash(string content)
        {
            const string mbTitle = $"Application Crash - {AssemblyInfo.Trademark}";
            try
            {
                return WPFMessageBox.Show(content, mbTitle,
                    WPFMessageBoxButton.OK,
                    WPFMessageBoxImage.Error);
            }
            catch
            {
                return null;
            }
        }
#endif
    }
}