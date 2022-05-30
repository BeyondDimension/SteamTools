namespace System.Application.Services.Implementation;

sealed class MainThreadPlatformServiceImpl : IMainThreadPlatformService
{
    bool IMainThreadPlatformService.PlatformIsMainThread => MainThread.IsMainThread;

    void IMainThreadPlatformService.PlatformBeginInvokeOnMainThread(Action action,
        ThreadingDispatcherPriority _) => MainThread.BeginInvokeOnMainThread(action);
}
