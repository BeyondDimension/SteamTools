using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;

namespace System.Application.Services.Implementation;

partial class ScheduledTaskServiceImpl
{
    /// <summary>
    /// 使用 TaskScheduler 库实现的开机启动
    /// </summary>
    /// <param name="platformService"></param>
    /// <param name="isAutoStart"></param>
    /// <param name="name"></param>
    /// <param name="userId"></param>
    /// <param name="tdName"></param>
    /// <param name="programName"></param>
    static void SetBootAutoStartByTaskScheduler(IPlatformService platformService, bool isAutoStart, string name, string userId, string tdName, string programName)
    {
        if (isAutoStart)
        {
            using var td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = name + " System Boot Run";
            td.Settings.Priority = ProcessPriorityClass.Normal;
            td.Settings.ExecutionTimeLimit = new TimeSpan(0);
            td.Settings.AllowHardTerminate = false;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Triggers.Add(new LogonTrigger { UserId = userId });
            td.Actions.Add(new ExecAction(programName,
                IPlatformService.SystemBootRunArguments,
                IOPath.BaseDirectory));
            if (platformService.IsAdministrator)
                td.Principal.RunLevel = TaskRunLevel.Highest;
            TaskService.Instance.RootFolder.RegisterTaskDefinition(tdName, td);
        }
        else
        {
            TaskService.Instance.RootFolder.DeleteTask(name, exceptionOnNotExists: false);
            TaskService.Instance.RootFolder.DeleteTask(tdName, exceptionOnNotExists: false);
        }
    }
}
