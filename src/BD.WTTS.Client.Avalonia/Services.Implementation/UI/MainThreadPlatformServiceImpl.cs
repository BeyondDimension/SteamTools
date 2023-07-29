namespace BD.WTTS.Services.Implementation;

sealed class MainThreadPlatformServiceImpl : IMainThreadPlatformService
{
    public bool PlatformIsMainThread
    {
        get
        {
            if (Startup.Instance.HasUI)
            {
                return Dispatcher.UIThread.CheckAccess();
            }
            else
            {
                return true;
            }
        }
    }

    public void PlatformBeginInvokeOnMainThread(Action action,
        ThreadingDispatcherPriority priority = ThreadingDispatcherPriority.Normal)
    {
        if (Startup.Instance.HasUI)
        {
            var priority_ = GetPriority(priority);
            Dispatcher.UIThread.Post(action, priority_);
        }
        else
        {
            action();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static DispatcherPriority GetPriority(ThreadingDispatcherPriority priority) => priority switch
    {
#pragma warning disable CS0618 // 类型或成员已过时
        ThreadingDispatcherPriority.Invalid or ThreadingDispatcherPriority.Inactive => DispatcherPriority.Inactive,
        ThreadingDispatcherPriority.SystemIdle => DispatcherPriority.SystemIdle,
        ThreadingDispatcherPriority.ApplicationIdle => DispatcherPriority.ApplicationIdle,
        ThreadingDispatcherPriority.ContextIdle => DispatcherPriority.ContextIdle,
        ThreadingDispatcherPriority.Background => DispatcherPriority.Background,
        ThreadingDispatcherPriority.Input => DispatcherPriority.Input,
        ThreadingDispatcherPriority.Loaded => DispatcherPriority.Loaded,
        ThreadingDispatcherPriority.Render => DispatcherPriority.Render,
        ThreadingDispatcherPriority.DataBind => DispatcherPriority.DataBind,
        ThreadingDispatcherPriority.Normal => DispatcherPriority.Normal,
        ThreadingDispatcherPriority.Send => DispatcherPriority.Send,
        _ => priority > ThreadingDispatcherPriority.Send ? DispatcherPriority.Send : DispatcherPriority.Inactive,
#pragma warning restore CS0618 // 类型或成员已过时
    };
}