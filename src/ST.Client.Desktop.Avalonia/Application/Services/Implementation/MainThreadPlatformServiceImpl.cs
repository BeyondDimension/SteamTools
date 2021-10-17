using Avalonia.Threading;

namespace System.Application.Services.Implementation
{
    internal sealed class MainThreadPlatformServiceImpl : IMainThreadPlatformService
    {
        public bool PlatformIsMainThread => Dispatcher.UIThread.CheckAccess();

        public void PlatformBeginInvokeOnMainThread(Action action,
            MainThread2.DispatcherPriority priority = MainThread2.DispatcherPriority.Normal)
        {
            var priority_ = GetPriority(priority);
            Dispatcher.UIThread.Post(action, priority_);
        }

        static DispatcherPriority GetPriority(MainThread2.DispatcherPriority priority) => priority switch
        {
            MainThread2.DispatcherPriority.SystemIdle => DispatcherPriority.SystemIdle,
            MainThread2.DispatcherPriority.ApplicationIdle => DispatcherPriority.ApplicationIdle,
            MainThread2.DispatcherPriority.ContextIdle => DispatcherPriority.ContextIdle,
            MainThread2.DispatcherPriority.Background => DispatcherPriority.Background,
            MainThread2.DispatcherPriority.Input => DispatcherPriority.Input,
            MainThread2.DispatcherPriority.Loaded => DispatcherPriority.Loaded,
            MainThread2.DispatcherPriority.Render => DispatcherPriority.Render,
            MainThread2.DispatcherPriority.Normal => DispatcherPriority.Normal,
            MainThread2.DispatcherPriority.Send => DispatcherPriority.Send,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
        };
    }
}