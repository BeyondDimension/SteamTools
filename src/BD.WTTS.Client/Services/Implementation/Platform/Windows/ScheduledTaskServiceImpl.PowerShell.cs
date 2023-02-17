#if WINDOWS
using static BD.WTTS.Services.IScheduledTaskService;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

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
    /// <param name="userName"></param>
    /// <param name="tdName"></param>
    /// <param name="programName"></param>
    static async void SetBootAutoStartByPowerShell(IPlatformService platformService, bool isAutoStart, string name, string userId, string userName, string tdName, string programName)
    {
        if (isAutoStart)
        {
            name = SecurityElement.Escape(name);
            userId = SecurityElement.Escape(userId);
            programName = SecurityElement.Escape(programName);
            var workingDirectory = SecurityElement.Escape(AppContext.BaseDirectory);
            var arguments = SecurityElement.Escape(IPlatformService.SystemBootRunArguments);
            if (string.IsNullOrWhiteSpace(userName)) userName = userId;
            else userName = SecurityElement.Escape(userName);
            var description = SecurityElement.Escape(GetDescription(name));
            var xml = $"<?xml version=\"1.0\" encoding=\"UTF-16\"?><Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\"><RegistrationInfo><Description>{description}</Description></RegistrationInfo><Triggers><LogonTrigger><Enabled>true</Enabled><UserId>{userName}</UserId></LogonTrigger></Triggers><Principals><Principal id=\"Author\"><UserId>{userId}</UserId><LogonType>InteractiveToken</LogonType><RunLevel>{(platformService.IsAdministrator ? "HighestAvailable" : "LeastPrivilege")}</RunLevel></Principal></Principals><Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><AllowHardTerminate>false</AllowHardTerminate><StartWhenAvailable>false</StartWhenAvailable><RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable><IdleSettings><Duration>PT10M</Duration><WaitTimeout>PT1H</WaitTimeout><StopOnIdleEnd>true</StopOnIdleEnd><RestartOnIdle>false</RestartOnIdle></IdleSettings><AllowStartOnDemand>true</AllowStartOnDemand><Enabled>true</Enabled><Hidden>false</Hidden><RunOnlyIfIdle>false</RunOnlyIfIdle><WakeToRun>false</WakeToRun><ExecutionTimeLimit>PT0S</ExecutionTimeLimit><Priority>5</Priority></Settings><Actions Context=\"Author\"><Exec><Command>{programName}</Command><Arguments>{arguments}</Arguments><WorkingDirectory>{workingDirectory}</WorkingDirectory></Exec></Actions></Task>";
            await RunPowerShellSinglePipeAsync($"Register-ScheduledTask -Force -TaskName '{Escape(tdName)}' -Xml '{Escape(xml)}';exit");
        }
        else
        {
            await RunPowerShellSinglePipeAsync($"Unregister-ScheduledTask -TaskName '{Escape(name)}' -Confirm:$false;Unregister-ScheduledTask -TaskName '{Escape(tdName)}' -Confirm:$false;exit");
        }
    }

    static string Escape(string value) => value.Replace("'", "''");

    static CancellationTokenSource? cts_runPowerShellSinglePipe = null;

    static async Task RunPowerShellSinglePipeAsync(string arguments)
    {
        Process? process = null;
        try
        {
            cts_runPowerShellSinglePipe?.Cancel();
            cts_runPowerShellSinglePipe = new CancellationTokenSource(9000);

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                Arguments = "-NoLogo",
                //RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
            };

            process = Process.Start(psi)!;
            //using var reader = process.StandardOutput;

            process.StandardInput.WriteLine(arguments.EndsWith(";exit", StringComparison.OrdinalIgnoreCase) ? arguments : $"{arguments};exit");

            //var result = reader.ReadToEnd();

            await process.WaitForExitAsync(cts_runPowerShellSinglePipe.Token);
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "RunPowerShell Fail, arguments: {0}.",
                arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
        }
        finally
        {
            if (process != null)
            {
                process.KillEntireProcessTree();
                process.Dispose();
            }
            cts_runPowerShellSinglePipe = null;
        }
    }
}
#endif