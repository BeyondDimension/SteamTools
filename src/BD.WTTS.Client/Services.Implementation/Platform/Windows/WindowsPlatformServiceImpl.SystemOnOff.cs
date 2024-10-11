#if WINDOWS
using Windows.ApplicationModel;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    public void SetSystemSessionEnding(Action action)
    {
        SystemEvents.SessionEnding += (sender, e) =>
        {
            if (e.Reason == SessionEndReasons.SystemShutdown)
            {
                action.Invoke();
            }
        };
    }

    /// <summary>
    /// UWP 的开机自启 TaskId，在 Package.appxmanifest 中设定
    /// </summary>
    const string BootAutoStartTaskId = "BootAutoStartTask";

    public async void SetBootAutoStart(bool isAutoStart, string name)
    {
        if (DesktopBridge.IsRunningAsUwp)
        {
            // https://blogs.windows.com/windowsdeveloper/2017/08/01/configure-app-start-log/
            // https://blog.csdn.net/lh275985651/article/details/109360162
            // https://www.cnblogs.com/wpinfo/p/uwp_auto_startup.html
            // 还需要通过 global::Windows.ApplicationModel.AppInstance.GetActivatedEventArgs() 判断为自启时最小化，不能通过参数启动
            var startupTask = await StartupTask.GetAsync(BootAutoStartTaskId);
            if (isAutoStart)
            {
                var startupTaskState = startupTask.State;
                if (startupTask.State == StartupTaskState.Disabled)
                {
                    startupTaskState = await startupTask.RequestEnableAsync();
                }
                if (startupTaskState != StartupTaskState.Enabled &&
                    startupTaskState != StartupTaskState.EnabledByPolicy)
                {
                    Toast.Show(ToastIcon.Error,
                        AppResources.SetBootAutoStartTrueFail_.Format(startupTaskState));
                }
            }
            else
            {
                startupTask.Disable();
            }
        }
        else
        {
            var isPrivilegedProcess = true;
            if (IsPrivilegedProcess)
            {
                var scheduledTaskService = IScheduledTaskService.Instance;
                scheduledTaskService.ThrowIsNull();
                scheduledTaskService.SetBootAutoStart(isAutoStart, name, isPrivilegedProcess: isPrivilegedProcess);
            }
            else
            {
                var ipc = await IPlatformService.IPCRoot.Instance;
                ipc.SetBootAutoStart(isAutoStart, name, isPrivilegedProcess: isPrivilegedProcess);
            }
        }
    }

    #region System Boot

    /// <summary>
    /// 锁定
    /// </summary>
    async void IPlatformService.SystemLock(int waitSecond)
    {
        await Task.Delay(waitSecond * 1000);
        Process2.Start("rundll32.exe", "user32.dll,LockWorkStation", true);
    }

    /// <summary>
    /// 关闭系统
    /// </summary>
    /// <param name="waitSecond">等待秒数</param>
    async void IPlatformService.SystemShutdown(int waitSecond)
    {
        await Task.Delay(waitSecond * 1000);
        Process2.Start("shutdown", "/s /t 0", true);
    }

    /// <summary>
    /// 睡眠系统
    /// </summary>
    /// <param name="waitSecond">等待秒数</param>
    async void IPlatformService.SystemSleep(int waitSecond)
    {
        await Task.Delay(waitSecond * 1000);
        Vanara.PInvoke.PowrProf.SetSuspendState(false, true, false);
    }

    /// <summary>
    /// 休眠系统
    /// </summary>
    /// <param name="waitSecond">等待秒数</param>
    async void IPlatformService.SystemHibernate(int waitSecond)
    {
        await Task.Delay(waitSecond * 1000);
        Vanara.PInvoke.PowrProf.SetSuspendState(true, true, false);
    }

    #endregion
}
#endif