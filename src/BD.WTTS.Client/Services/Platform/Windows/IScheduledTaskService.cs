#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface IScheduledTaskService
{
    const string TAG = "ScheduledTaskS";

    static IScheduledTaskService? Instance => Ioc.Get_Nullable<IScheduledTaskService>();

    /// <summary>
    /// 开机启动使用 taskschd.msc 实现
    /// </summary>
    /// <param name="platformService"></param>
    /// <param name="isAutoStart"></param>
    /// <param name="name"></param>
    void SetBootAutoStart(IPlatformService platformService, bool isAutoStart, string name);
}
#endif