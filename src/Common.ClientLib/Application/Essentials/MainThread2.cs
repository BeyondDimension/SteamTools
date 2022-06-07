using System.Application.Services;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Application;

/// <summary>
/// 适用于桌面端的主线程帮助类，参考 Xamarin.Essentials.MainThread。
/// <para><see cref="https://docs.microsoft.com/zh-cn/xamarin/essentials/main-thread"/></para>
/// <para><see cref="https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/MainThread/MainThread.shared.cs"/></para>
/// </summary>
public static class MainThread2
{
    /// <summary>
    /// 获取当前是否为主线程。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMainThread() => IMainThreadPlatformService.Instance.PlatformIsMainThread;

    /// <summary>
    /// 调用应用程序主线程上的操作。
    /// </summary>
    /// <param name="action">要执行的操作。</param>
    /// <param name="priority"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BeginInvokeOnMainThread(Action action, ThreadingDispatcherPriority priority = ThreadingDispatcherPriority.Normal)
    {
        if (IsMainThread())
        {
            action();
        }
        else
        {
            try
            {
                IMainThreadPlatformService.Instance.PlatformBeginInvokeOnMainThread(action, priority);
            }
            catch (InvalidOperationException)
            {
                // https://github.com/dotnet/maui/blob/48840b8dd4f63e298ac63af7f9696f7e0581589c/src/Essentials/src/MainThread/MainThread.uwp.cs#L16-L20
                action();
            }
        }
    }

    /// <summary>
    /// 异步调用主线程。
    /// </summary>
    /// <param name="action"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    public static Task InvokeOnMainThreadAsync(Action action, ThreadingDispatcherPriority priority = ThreadingDispatcherPriority.Normal)
    {
        if (IsMainThread())
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

    /// <summary>
    /// 异步调用主线程。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func, ThreadingDispatcherPriority priority = ThreadingDispatcherPriority.Normal)
    {
        if (IsMainThread())
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

    /// <summary>
    /// 异步调用主线程。
    /// </summary>
    /// <param name="funcTask"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    public static Task InvokeOnMainThreadAsync(Func<Task> funcTask, ThreadingDispatcherPriority priority = ThreadingDispatcherPriority.Normal)
    {
        if (IsMainThread())
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

    /// <summary>
    /// 异步调用主线程。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="funcTask"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask, ThreadingDispatcherPriority priority = ThreadingDispatcherPriority.Normal)
    {
        if (IsMainThread())
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