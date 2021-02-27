using Avalonia.Threading;

namespace System.Application.Services.Implementation
{
    internal sealed class MainThreadPlatformServiceImpl : IMainThreadPlatformService
    {
        public bool PlatformIsMainThread => Dispatcher.UIThread.CheckAccess();

        public void PlatformBeginInvokeOnMainThread(Action action,
            MainThreadDesktop.DispatcherPriority? priority = null)
        {
            if (priority.HasValue)
            {
                var priority_ = GetPriority(priority.Value);
                Dispatcher.UIThread.Post(action, priority_);
            }
            else
            {
                Dispatcher.UIThread.Post(action);
            }
        }

        static DispatcherPriority GetPriority(MainThreadDesktop.DispatcherPriority priority) => priority switch
        {
            MainThreadDesktop.DispatcherPriority.SystemIdle => DispatcherPriority.SystemIdle,
            MainThreadDesktop.DispatcherPriority.ApplicationIdle => DispatcherPriority.ApplicationIdle,
            MainThreadDesktop.DispatcherPriority.ContextIdle => DispatcherPriority.ContextIdle,
            MainThreadDesktop.DispatcherPriority.Background => DispatcherPriority.Background,
            MainThreadDesktop.DispatcherPriority.Input => DispatcherPriority.Input,
            MainThreadDesktop.DispatcherPriority.Loaded => DispatcherPriority.Loaded,
            MainThreadDesktop.DispatcherPriority.Render => DispatcherPriority.Render,
            MainThreadDesktop.DispatcherPriority.Normal => DispatcherPriority.Normal,
            MainThreadDesktop.DispatcherPriority.Send => DispatcherPriority.Send,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
        };
    }
}