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
            var userName = identity.Name;
            var userId = hasSid ? identity.User!.ToString() : userName;
            var tdName = hasSid ? userId : userId.Replace(Path.DirectorySeparatorChar, '_');
            tdName = $"{name}_{{{tdName}}}";
            SetBootAutoStartByPowerShell(platformService, isAutoStart, name, userId, userName, tdName, IApplication.ProgramName);
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "SetBootAutoStart Fail, isAutoStart: {0}, name: {1}.", isAutoStart, name);
        }
    }

    static string GetDescription(string name) => name + " System Boot Run";
}
