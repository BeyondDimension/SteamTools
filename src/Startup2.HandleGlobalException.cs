using NLog;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI;
using System.Collections.Generic;
using System.Reactive;
#if !__MOBILE__
using AvaloniaApplication = Avalonia.Application;
#endif
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace System.Application
{
    partial class Startup
    {
        static readonly HashSet<Exception> exceptions = new();
        static readonly object lock_global_ex_log = new();
        static readonly Lazy<Logger> _logger = new(LogManager.GetCurrentClassLogger);

        static Logger? Logger => _logger.Value;

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
        public static void InitGlobalExceptionHandler()
        {
#if __ANDROID__
            Java.Lang.Thread.DefaultUncaughtExceptionHandler = new UncaughtExceptionHandler((_, ex) =>
            {
                GlobalExceptionHandler(ex, nameof(Java));
            }, Java.Lang.Thread.DefaultUncaughtExceptionHandler);
#else
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    GlobalExceptionHandler(ex, nameof(AppDomain), e.IsTerminating);
                }
            };
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
            {
                // https://github.com/AvaloniaUI/Avalonia/issues/5290#issuecomment-760751036
                GlobalExceptionHandler(ex, nameof(RxApp));
            });
#endif
        }

        /// <summary>
        /// 全局异常处理
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="name"></param>
        /// <param name="isTerminating"></param>
        public static void GlobalExceptionHandler(Exception ex, string name, bool? isTerminating = null)
        {
            lock (lock_global_ex_log)
            {
                if (!exceptions.Add(ex)) return;
            }

            IApplication.TrySetLoggerMinLevel(LogLevel.Trace);
            // NLog: catch any exception and log it.
            Logger?.Error(ex, "Stopped program because of exception, name: {1}, isTerminating: {0}", isTerminating, name);

#if !__MOBILE__
            try
            {
                DI.Get_Nullable<IHttpProxyService>()?.StopProxy();
                ProxyService.OnExitRestoreHosts();
            }
            catch (Exception ex_restore_hosts)
            {
                Logger?.Error(ex_restore_hosts, "(App)Close exception when OnExitRestoreHosts");
            }
#endif

#if !__MOBILE__
            //var callTryShutdown = true;
            try
            {
                /*callTryShutdown = !*/
                App.Shutdown();
            }
            catch (Exception ex_shutdown)
            {
                Logger?.Error(ex_shutdown,
                    "(App)Close exception when exception occurs");
            }

            //if (callTryShutdown)
            //{
            //    try
            //    {
            //        AppHelper.TryShutdown();
            //    }
            //    catch (Exception ex_shutdown_app_helper)
            //    {
            //        Logger?.Error(ex_shutdown_app_helper,
            //            "(AppHelper)Close exception when exception occurs");
            //    }
            //}

            try
            {
                if (AvaloniaApplication.Current is IApplication app)
                {
                    app.CompositeDisposable.Dispose();
                }
            }
            catch
            {
            }
#endif
        }
    }
}