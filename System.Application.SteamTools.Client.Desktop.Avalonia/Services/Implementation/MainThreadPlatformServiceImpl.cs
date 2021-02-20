using Avalonia.Threading;

namespace System.Application.Services.Implementation
{
    internal sealed class MainThreadPlatformServiceImpl : IMainThreadPlatformService
    {
        public bool PlatformIsMainThread => Dispatcher.UIThread.CheckAccess();

        public void PlatformBeginInvokeOnMainThread(Action action) => Dispatcher.UIThread.Post(action);
    }
}