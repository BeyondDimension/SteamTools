#if WINDOWS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    /// <summary>
    /// 检查指定的进程是否以管理员权限运行
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsProcessElevated(Process process)
    {
        try
        {
            var handle = process.Handle;
            using WindowsIdentity identity = new(handle);
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Win32Exception ex)
        {
            /* “process.Handle”引发了类型“System.ComponentModel.Win32Exception”的异常
             * Data: {System.Collections.ListDictionaryInternal}
             * ErrorCode: -2147467259
             * HResult: -2147467259
             * HelpLink: null
             * InnerException: null
             * Message: "拒绝访问。"
             * NativeErrorCode: 5
             * Source: "System.Diagnostics.Process"
             */
            if (ex.NativeErrorCode == 5)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 使用 BypassUAC 以管理员权限启动进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static async Task<Process?> StartAsAdministratorByBypassUAC(string fileName, string? arguments = null)
    {
        /** 33. Author: winscripting.blog
         * https://github.com/hfiref0x/UACME
         * https://github.com/FatRodzianko/SharpBypassUAC/blob/67a9a3e4500731881f60863454e4e42ef46874e0/SharpBypassUAC/FodHelper.cs
         * Type: Shell API
         * Method: Registry key manipulation
         * Target(s): \system32\fodhelper.exe
         * Component(s): Attacker defined
         * Implementation: ucmShellRegModMethod
         * Works from: Windows 10 TH1 (10240)
         * Fixed in: unfixed 🙈
         * How: -
         * Code status: added in v2.7.2
         */
        RegistryKey? classes = null;
        var targetName = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.System),
            "fodhelper.exe");
        try
        {
            if (!File.Exists(targetName))
                return null;

            var processPath = Environment.ProcessPath;
            processPath.ThrowIsNull();
            if (fileName != processPath) // 仅允许启动自己
                throw new ArgumentOutOfRangeException(nameof(fileName));

            var processName = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrWhiteSpace(processName))
                throw new ArgumentNullException(nameof(processName));

            classes = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
            using var command = classes!.CreateSubKey(@"ms-settings\Shell\Open\command", true);
            command.SetValue("DelegateExecute", "");
            command.SetValue("", string.IsNullOrWhiteSpace(arguments) ?
                $"\"{fileName}\"" : $"\"{fileName}\" {arguments}");

            ProcessStartInfo psi = new()
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = targetName,
            };
            var processs_start_before = new HashSet<int>(
                Process.GetProcessesByName(processName).Select(x => x.Id));
            var process = Process.Start(psi);
            process!.WaitForExit(6000);

            for (int i = 0; i < 7; i++)
            {
                var processs_start_after = Process.GetProcessesByName(processName);
                foreach (var item in processs_start_after)
                {
                    if (processs_start_before.Contains(item.Id))
                        continue;

                    if (IsProcessElevated(item))
                        return item;
                }

                await Task.Delay(800);
            }

            return null;
        }
        catch
        {

        }
        finally
        {
            if (classes != null)
            {
                try
                {
                    classes.DeleteSubKeyTree("ms-settings");
                }
                catch
                {

                }
                classes.Dispose();
            }
        }
        return null;
    }

    /// <summary>
    /// 以管理员权限启动进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    internal static async ValueTask<Process?> StartAsAdministrator(string fileName, string? arguments = null)
    {
#if !DEBUG
        if (IPlatformService._IsAdministrator.Value)
            return Process2.Start(fileName, arguments);
#endif

        var process = await StartAsAdministratorByBypassUAC(fileName, arguments);
        if (process != null)
            return process;

        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas",
            };
            var process_runas = Process.Start(psi);
            return process_runas;
        }
        catch
        {
            return null;
        }
    }
}
#endif