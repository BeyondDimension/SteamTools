using System.Application.UI;
using System.Security.Principal;
using static System.Application.Services.IScheduledTaskService;

namespace System.Application.Services.Implementation;

sealed partial class ScheduledTaskServiceImpl : IScheduledTaskService
{
    void IScheduledTaskService.SetBootAutoStart(IPlatformService platformService, bool isAutoStart, string name)
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var hasSid = identity.User?.IsAccountSid() ?? false;
            var userId = hasSid ? identity.User!.ToString() : identity.Name;
            var tdName = hasSid ? userId : userId.Replace(Path.DirectorySeparatorChar, '_');
            tdName = $"{name}_{{{tdName}}}";
            SetBootAutoStartByTaskScheduler(platformService, isAutoStart, name, userId, tdName, IApplication.ProgramName);
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "SetBootAutoStart Fail, isAutoStart: {0}, name: {1}.", isAutoStart, name);
        }
    }
}
