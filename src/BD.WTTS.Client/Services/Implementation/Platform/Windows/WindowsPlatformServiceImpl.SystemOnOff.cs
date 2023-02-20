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
            action.Invoke();
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
                    Toast.Show(AppResources.SetBootAutoStartTrueFail_.Format(startupTaskState));
                }
            }
            else
            {
                startupTask.Disable();
            }
        }
        else
        {
            var scheduledTaskService = IScheduledTaskService.Instance;
            scheduledTaskService.ThrowIsNull();
            scheduledTaskService.SetBootAutoStart(this, isAutoStart, name);
        }
    }
}
#endif