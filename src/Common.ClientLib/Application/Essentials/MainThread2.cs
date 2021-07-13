using System.Threading.Tasks;
using System.Windows.Threading;
using Xamarin.Essentials;
using static System.Application.Services.IMainThreadPlatformService;

namespace System.Application
{
    /// <summary>
    /// 适用于桌面端的主线程帮助类，参考 Xamarin.Essentials.MainThread
    /// <para><see cref="https://docs.microsoft.com/zh-cn/xamarin/essentials/main-thread"/></para>
    /// <para><see cref="https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/MainThread/MainThread.shared.cs"/></para>
    /// </summary>
    public static class MainThread2
    {
        public static bool IsMainThread
        {
            get
            {
                if (XamarinEssentials.IsSupported)
                {
                    return MainThread.IsMainThread;
                }
                else
                {
                    return Instance.PlatformIsMainThread;
                }
            }
        }

        public static void BeginInvokeOnMainThread(Action action, DispatcherPriorityCompat priority = DispatcherPriorityCompat.Normal)
        {
            if (XamarinEssentials.IsSupported)
            {
                MainThread.BeginInvokeOnMainThread(action);
            }
            else
            {
                if (IsMainThread)
                {
                    action();
                }
                else
                {
                    Instance.PlatformBeginInvokeOnMainThread(action, priority);
                }
            }
        }

        public static Task InvokeOnMainThreadAsync(Action action, DispatcherPriorityCompat priority = DispatcherPriorityCompat.Normal)
        {
            if (XamarinEssentials.IsSupported)
            {
                return MainThread.InvokeOnMainThreadAsync(action);
            }
            else
            {
                if (IsMainThread)
                {
                    action();
#if NETSTANDARD1_0
                    return Task.FromResult(true);
#else
                    return Task.CompletedTask;
#endif
                }

                var tcs = new TaskCompletionSource<bool>();

                BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        action();
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                }, priority);

                return tcs.Task;
            }
        }

        public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func, DispatcherPriorityCompat priority = DispatcherPriorityCompat.Normal)
        {
            if (XamarinEssentials.IsSupported)
            {
                return MainThread.InvokeOnMainThreadAsync(func);
            }
            else
            {
                if (IsMainThread)
                {
                    return Task.FromResult(func());
                }

                var tcs = new TaskCompletionSource<T>();

                BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        var result = func();
                        tcs.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                }, priority);

                return tcs.Task;
            }
        }

        public static Task InvokeOnMainThreadAsync(Func<Task> funcTask, DispatcherPriorityCompat priority = DispatcherPriorityCompat.Normal)
        {
            if (XamarinEssentials.IsSupported)
            {
                return MainThread.InvokeOnMainThreadAsync(funcTask);
            }
            else
            {
                if (IsMainThread)
                {
                    return funcTask();
                }

                var tcs = new TaskCompletionSource<object?>();

                BeginInvokeOnMainThread(
                    async () =>
                    {
                        try
                        {
                            await funcTask().ConfigureAwait(false);
                            tcs.SetResult(null);
                        }
                        catch (Exception e)
                        {
                            tcs.SetException(e);
                        }
                    }, priority);

                return tcs.Task;
            }
        }

        public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask, DispatcherPriorityCompat priority = DispatcherPriorityCompat.Normal)
        {
            if (XamarinEssentials.IsSupported)
            {
                return MainThread.InvokeOnMainThreadAsync(funcTask);
            }
            else
            {
                if (IsMainThread)
                {
                    return funcTask();
                }

                var tcs = new TaskCompletionSource<T>();

                BeginInvokeOnMainThread(
                    async () =>
                    {
                        try
                        {
                            var ret = await funcTask().ConfigureAwait(false);
                            tcs.SetResult(ret);
                        }
                        catch (Exception e)
                        {
                            tcs.SetException(e);
                        }
                    }, priority);

                return tcs.Task;
            }
        }
    }
}

namespace System.Windows.Threading
{
    /// <summary>
    ///     An enunmeration describing the priorities at which
    ///     operations can be invoked via the Dispatcher.
    ///     <see cref="https://github.com/dotnet/wpf/blob/master/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherPriority.cs"/>
    /// </summary>
    ///
    public enum DispatcherPriorityCompat
    {
        /// <summary>
        ///     Operations at this priority are processed when the system
        ///     is idle.
        /// </summary>
        SystemIdle,

        /// <summary>
        ///     Minimum possible priority
        /// </summary>
        MinValue = SystemIdle,

        /// <summary>
        ///     Operations at this priority are processed when the application
        ///     is idle.
        /// </summary>
        ApplicationIdle,

        /// <summary>
        ///     Operations at this priority are processed when the context
        ///     is idle.
        /// </summary>
        ContextIdle,

        /// <summary>
        ///     Operations at this priority are processed after all other
        ///     non-idle operations are done.
        /// </summary>
        Background,

        /// <summary>
        ///     Operations at this priority are processed at the same
        ///     priority as input.
        /// </summary>
        Input,

        /// <summary>
        ///     Operations at this priority are processed when layout and render is
        ///     done but just before items at input priority are serviced. Specifically
        ///     this is used while firing the Loaded event
        /// </summary>
        Loaded,

        /// <summary>
        ///     Operations at this priority are processed at the same
        ///     priority as rendering.
        /// </summary>
        Render,

        /// <summary>
        ///     Operations at this priority are processed at normal priority.
        /// </summary>
        Normal,

        /// <summary>
        ///     Operations at this priority are processed before other
        ///     asynchronous operations.
        /// </summary>
        Send,

        /// <summary>
        ///     Maximum possible priority
        /// </summary>
        MaxValue = Send,
    }
}