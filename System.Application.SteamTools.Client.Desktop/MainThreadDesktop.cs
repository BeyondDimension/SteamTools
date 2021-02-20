using System.Threading.Tasks;
using static System.Application.Services.IMainThreadPlatformService;

namespace System.Application
{
    /// <summary>
    /// 适用于桌面端的主线程帮助类，参考 Xamarin.Essentials.MainThread
    /// <para><see cref="https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/MainThread/MainThread.shared.cs"/></para>
    /// </summary>
    public static class MainThreadDesktop
    {
        public static bool IsMainThread => Instance.PlatformIsMainThread;

        public static void BeginInvokeOnMainThread(Action action)
        {
            if (IsMainThread)
            {
                action();
            }
            else
            {
                Instance.PlatformBeginInvokeOnMainThread(action);
            }
        }

        public static Task InvokeOnMainThreadAsync(Action action)
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
            });

            return tcs.Task;
        }

        public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
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
            });

            return tcs.Task;
        }

        public static Task InvokeOnMainThreadAsync(Func<Task> funcTask)
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
                });

            return tcs.Task;
        }

        public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
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
                });

            return tcs.Task;
        }
    }
}