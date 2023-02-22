namespace BD.WTTS.Services.Implementation;

sealed class MainThreadPlatformServiceImpl : IMainThreadPlatformService
{
    public bool PlatformIsMainThread => Dispatcher.UIThread.CheckAccess();

    public void PlatformBeginInvokeOnMainThread(Action action,
        ThreadingDispatcherPriority priority = ThreadingDispatcherPriority.Normal)
    {
        var priority_ = GetPriority(priority);
        Dispatcher.UIThread.Post(action, priority_);
    }

    static DispatcherPriority GetPriority(ThreadingDispatcherPriority priority) => priority switch
    {
        ThreadingDispatcherPriority.Invalid or ThreadingDispatcherPriority.Inactive => DispatcherPriority.MinValue,
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
        _ => priority > ThreadingDispatcherPriority.Send ? DispatcherPriority.MaxValue : DispatcherPriority.MinValue,
    };
}