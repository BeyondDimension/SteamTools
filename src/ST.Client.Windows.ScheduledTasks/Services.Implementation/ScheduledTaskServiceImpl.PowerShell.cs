using System.Diagnostics;

namespace System.Application.Services.Implementation;

partial class ScheduledTaskServiceImpl
{
    // https://github.com/PowerShell/PowerShell/issues/13540
    // Microsoft.PowerShell.SDK 不支持单文件发布

    /// <summary>
    /// 使用 PowerShell 实现的开机启动
    /// <para>https://docs.microsoft.com/en-us/powershell/module/scheduledtasks</para>
    /// </summary>
    /// <param name="platformService"></param>
    /// <param name="isAutoStart"></param>
    /// <param name="name"></param>
    /// <param name="userId"></param>
    /// <param name="tdName"></param>
    /// <param name="programName"></param>
    static void SetBootAutoStartByPowerShell(IPlatformService platformService, bool isAutoStart, string name, string userId, string tdName, string programName)
    {
        if (isAutoStart)
        {

        }
        else
        {
            RunPowerShell($"Unregister-ScheduledTask -TaskName '{Escape(name)}' -Confirm:$false;Unregister-ScheduledTask -TaskName '{Escape(tdName)}' -Confirm:$false");
        }
    }

    static string Escape(string value) => value.Replace("'", "''");

    static string RunPowerShell(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            Arguments = arguments,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)!;
        using var reader = process.StandardOutput;

        process.EnableRaisingEvents = true;

        var result = reader.ReadToEnd();
        return result;
    }
}
