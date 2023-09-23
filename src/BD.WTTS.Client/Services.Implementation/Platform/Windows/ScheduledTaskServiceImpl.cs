#if WINDOWS
using static BD.WTTS.Services.IScheduledTaskService;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed partial class ScheduledTaskServiceImpl : IScheduledTaskService
{
    bool IScheduledTaskService.SetBootAutoStart(bool isAutoStart, string name, bool? isPrivilegedProcess)
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var hasSid = identity.User?.IsAccountSid() ?? false;
            var userName = identity.Name;
            var userId = hasSid ? identity.User!.ToString() : userName;
            var tdName = hasSid ? userId : userId.Replace(Path.DirectorySeparatorChar, '_');
            tdName = $"{name}_{{{tdName}}}";
            SetBootAutoStartByPowerShell(isAutoStart, name, userId, userName, tdName, IApplication.ProgramName, isPrivilegedProcess: isPrivilegedProcess);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "SetBootAutoStart Fail, isAutoStart: {0}, name: {1}.", isAutoStart, name);
            return false;
        }
    }

    static string GetDescription(string name) => name + " System Boot Run";
}
#endif