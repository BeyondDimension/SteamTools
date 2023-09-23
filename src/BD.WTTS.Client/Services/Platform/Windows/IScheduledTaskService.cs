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
    /// <param name="isAutoStart">开启或关闭</param>
    /// <param name="name">任务名词</param>
    /// <param name="isPrivilegedProcess">是否需要管理员权限启动，为 <see langword="null"/> 时将使用当前执行进程的权限判断</param>
    /// <returns></returns>
    bool SetBootAutoStart(bool isAutoStart, string name, bool? isPrivilegedProcess = null);
}
#endif