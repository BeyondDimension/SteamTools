using System.Reactive;
using Logger = NLog.Logger;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    /// <summary>
    /// 全局异常助手类
    /// </summary>
    static class GlobalExceptionHelpers
    {
        const string TAG = nameof(GlobalExceptionHelpers);

        static int index_exceptions;
        static readonly int[] exceptions = new int[6];
        static readonly object lock_global_ex_log = new();
        static readonly Lazy<Logger> _logger = new(LogManager.GetCurrentClassLogger);

        static Logger Logger => _logger.Value;

#if __ANDROID__
        sealed class UncaughtExceptionHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
        {
            readonly Action<Java.Lang.Thread, Java.Lang.Throwable> action;
            readonly Java.Lang.Thread.IUncaughtExceptionHandler? @interface;

            public UncaughtExceptionHandler(Action<Java.Lang.Thread, Java.Lang.Throwable> action, Java.Lang.Thread.IUncaughtExceptionHandler? @interface = null)
            {
                this.action = action;
                this.@interface = @interface;
            }

            public void UncaughtException(Java.Lang.Thread t, Java.Lang.Throwable e)
            {
                @interface?.UncaughtException(t, e);
                action(t, e);
            }
        }
#endif

        /// <summary>
        /// 初始化全局异常处理
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Init()
        {
#if __ANDROID__
            Java.Lang.Thread.DefaultUncaughtExceptionHandler = new UncaughtExceptionHandler((_, ex) =>
            {
                Handler(ex, nameof(Java));
            }, Java.Lang.Thread.DefaultUncaughtExceptionHandler);
#else
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
#endif
        }

        /// <summary>
        /// 全局异常处理
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="name"></param>
        /// <param name="isTerminating"></param>
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
#if DEBUG
            Console.WriteLine($"Stopped program because of exception, name: {name}, isTerminating: {isTerminating}");
#endif
            Logger.Error(ex, "Stopped program because of exception, name: {1}, isTerminating: {0}", isTerminating, name);

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            if (StartupOptions.Value.HasPlugins)
            {
                foreach (var plugin in StartupOptions.Value.Plugins!)
                {
                    try
                    {
                        plugin.OnUnhandledException(ex, name, isTerminating);
                    }
                    catch (Exception ex_restore_hosts)
                    {
#if DEBUG
                        Console.WriteLine("(App)Close exception when OnExitRestoreHosts");
                        Console.WriteLine(ex_restore_hosts);
#endif
                        Logger.Error(ex_restore_hosts,
                            "(App)Close exception when OnExitRestoreHosts");
                    }
                }
            }
#endif
        }
    }
}