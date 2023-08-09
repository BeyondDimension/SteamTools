#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    /// <summary>
    /// 检查进程是否作为 Windows 服务托管
    /// </summary>
    /// <param name="processId">进程 Id，为 <see langword="null"/> 则为当前进程</param>
    /// <returns>如果进程作为 Windows 服务托管，则为 <see langword="true" />，否则为 <see langword="false" /></returns>
    public static bool IsWindowsService(int? processId = default)
    {
        // https://github.com/dotnet/runtime/blob/v7.0.10/src/libraries/Microsoft.Extensions.Hosting.WindowsServices/src/WindowsServiceHelpers.cs#L20
        if (!processId.HasValue)
        {
            processId =
#if NET5_0_OR_GREATER
                Environment.ProcessId;
#else
                Process.GetCurrentProcess().Id;
#endif
        }
        var parent = GetParentProcess(processId.Value);
        if (parent == null)
        {
            return false;
        }
        return string.Equals("services", parent.ProcessName, StringComparison.OrdinalIgnoreCase);
    }

    static unsafe Process? GetParentProcess(int processId)
    {
        // https://github.com/dotnet/runtime/blob/v7.0.10/src/libraries/Microsoft.Extensions.Hosting.WindowsServices/src/Internal/Win32.cs#L14

        PInvoke.Kernel32.SafeObjectHandle? snapshotHandle = default;
        try
        {
            // Get a list of all processes
            snapshotHandle = PInvoke.Kernel32.CreateToolhelp32Snapshot(PInvoke.Kernel32.CreateToolhelp32SnapshotFlags.TH32CS_SNAPPROCESS, 0);

            PInvoke.Kernel32.PROCESSENTRY32 procEntry = default;
            procEntry.dwSize = sizeof(PInvoke.Kernel32.PROCESSENTRY32);
            if (PInvoke.Kernel32.Process32First(snapshotHandle, &procEntry))
            {
                do
                {
                    if (processId == procEntry.th32ProcessID)
                    {
                        return Process.GetProcessById(procEntry.th32ParentProcessID);
                    }
                }
                while (PInvoke.Kernel32.Process32Next(snapshotHandle, &procEntry));
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            snapshotHandle?.Dispose();
        }

        return null;
    }
}
#endif