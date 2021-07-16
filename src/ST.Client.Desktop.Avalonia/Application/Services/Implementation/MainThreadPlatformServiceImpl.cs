using Avalonia.Threading;
using System.Windows.Threading;

namespace System.Application.Services.Implementation
{
    internal sealed class MainThreadPlatformServiceImpl : IMainThreadPlatformService
    {
        public bool PlatformIsMainThread => Dispatcher.UIThread.CheckAccess();

        public void PlatformBeginInvokeOnMainThread(Action action,
            DispatcherPriorityCompat priority = DispatcherPriorityCompat.Normal)
        {
            var priority_ = GetPriority(priority);
            Dispatcher.UIThread.Post(action, priority_);
        }

        static DispatcherPriority GetPriority(DispatcherPriorityCompat priority) => priority switch
        {
            DispatcherPriorityCompat.SystemIdle => DispatcherPriority.SystemIdle,
            DispatcherPriorityCompat.ApplicationIdle => DispatcherPriority.ApplicationIdle,
            DispatcherPriorityCompat.ContextIdle => DispatcherPriority.ContextIdle,
            DispatcherPriorityCompat.Background => DispatcherPriority.Background,
            DispatcherPriorityCompat.Input => DispatcherPriority.Input,
            DispatcherPriorityCompat.Loaded => DispatcherPriority.Loaded,
            DispatcherPriorityCompat.Render => DispatcherPriority.Render,
            DispatcherPriorityCompat.Normal => DispatcherPriority.Normal,
            DispatcherPriorityCompat.Send => DispatcherPriority.Send,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
        };
    }
}