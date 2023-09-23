#if WINDOWS
using static BD.WTTS.Services.IScheduledTaskService;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class ScheduledTaskServiceImpl
{
    // https://github.com/PowerShell/PowerShell/issues/13540
    // Microsoft.PowerShell.SDK 不支持单文件发布

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetScheduledTaskXml(string comment, string description, string userName, string userId, bool isPrivileged, string programName, string arguments, string workingDirectory)
    {
        var xml = $"<?xml version=\"1.0\" encoding=\"UTF-16\"?><Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\"><!-- {comment} --><RegistrationInfo><Description>{description}</Description></RegistrationInfo><Triggers><LogonTrigger><Enabled>true</Enabled><UserId>{userName}</UserId></LogonTrigger></Triggers><Principals><Principal id=\"Author\"><UserId>{userId}</UserId><LogonType>InteractiveToken</LogonType><RunLevel>{(isPrivileged ? "HighestAvailable" : "LeastPrivilege")}</RunLevel></Principal></Principals><Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><AllowHardTerminate>false</AllowHardTerminate><StartWhenAvailable>false</StartWhenAvailable><RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable><IdleSettings><Duration>PT10M</Duration><WaitTimeout>PT1H</WaitTimeout><StopOnIdleEnd>true</StopOnIdleEnd><RestartOnIdle>false</RestartOnIdle></IdleSettings><AllowStartOnDemand>true</AllowStartOnDemand><Enabled>true</Enabled><Hidden>false</Hidden><RunOnlyIfIdle>false</RunOnlyIfIdle><WakeToRun>false</WakeToRun><ExecutionTimeLimit>PT0S</ExecutionTimeLimit><Priority>5</Priority></Settings><Actions Context=\"Author\"><Exec><Command>{programName}</Command><Arguments>{arguments}</Arguments><WorkingDirectory>{workingDirectory}</WorkingDirectory></Exec></Actions></Task>";
        return xml;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static async Task RegisterScheduledTask(string taskName, string xml)
    {
        taskName = Escape(taskName);
        xml = Escape(xml);
        await RunPowerShellSinglePipeAsync($"Register-ScheduledTask -Force -TaskName '{taskName}' -Xml '{xml}';exit");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static async Task UnregisterScheduledTask(IEnumerable<string>? taskNames)
    {
        if (!taskNames.Any_Nullable()) return;

        StringBuilder builder = new();
        foreach (var taskName in taskNames)
        {
            var taskName_ = Escape(taskName);
            builder.Append($"Unregister-ScheduledTask -TaskName '{taskName_}' -Confirm:$false");
            builder.Append(';');
        }
        builder.Append("exit");
        var arguments = builder.ToString();
        await RunPowerShellSinglePipeAsync(arguments);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Task UnregisterScheduledTask(params string[] taskNames) => UnregisterScheduledTask(taskNames.AsEnumerable());

    /// <summary>
    /// 使用 PowerShell 实现的开机启动
    /// <para>https://docs.microsoft.com/en-us/powershell/module/scheduledtasks</para>
    /// </summary>
    /// <param name="isAutoStart"></param>
    /// <param name="name"></param>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    /// <param name="taskName"></param>
    /// <param name="programName"></param>
    static async void SetBootAutoStartByPowerShell(bool isAutoStart, string name, string userId, string userName, string taskName, string programName, bool? isPrivilegedProcess = null)
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
            isPrivilegedProcess ??= WindowsPlatformServiceImpl.IsPrivilegedProcess;
            var xml = GetScheduledTaskXml(nameof(SetBootAutoStartByPowerShell), description, userName, userId, isPrivilegedProcess.Value, programName, arguments, workingDirectory);
            await RegisterScheduledTask(taskName, xml);
        }
        else
        {
            await UnregisterScheduledTask(name, taskName);
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
                try
                {
                    process.KillEntireProcessTree();
                }
                catch
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {

                    }
                }
                try
                {
                    process.Dispose();
                }
                catch
                {

                }
            }
            cts_runPowerShellSinglePipe = null;
        }
    }
}
#endif