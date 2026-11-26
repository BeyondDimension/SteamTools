#if WINDOWS
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    public byte[] GetCpuSchedulingTopology() => WindowsCpuScheduling.ReadTopology();

    public CpuProcessState GetCpuSchedulingProcessState(int processId) => WindowsCpuScheduling.ReadProcess(processId);

    public bool IsCpuSchedulingProtectedProcess(int processId, string processName) =>
        WindowsCpuScheduling.IsProtectedName(processId, processName);

    public Task<int> SetCpuSchedulingAsync(int processId, long creationTime, int priority, ulong affinityMask,
        int ecoMode, int memoryPriority, bool restoreDefaults) => Task.FromResult(
        WindowsCpuScheduling.Apply(processId, creationTime, priority, affinityMask, ecoMode, memoryPriority, restoreDefaults));

    public async Task<int> ApplyCpuSchedulingAsync(int processId, long creationTime, int priority, ulong affinityMask,
        int ecoMode, int memoryPriority, bool restoreDefaults)
    {
        var result = await SetCpuSchedulingAsync(processId, creationTime, priority, affinityMask, ecoMode, memoryPriority, restoreDefaults);
        var helper = IPlatformService.IPCRoot.Instance;
        if (result != CpuSchedulingResult.AccessDenied || IsPrivilegedProcess || !helper.IsCompletedSuccessfully)
            return result;

        try
        {
            return await helper.Result.SetCpuSchedulingAsync(processId, creationTime, priority, affinityMask,
                ecoMode, memoryPriority, restoreDefaults);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(WindowsCpuScheduling), ex, "CPU scheduling IPC");
            return result;
        }
    }
}

static unsafe partial class WindowsCpuScheduling
{
    const int ErrorInsufficientBuffer = 122;
    const uint ProcessSetInformation = 0x0200;
    const uint ProcessQueryInformation = 0x0400;
    const uint ProcessQueryLimitedInformation = 0x1000;
    const int ProcessMemoryPriority = 0;
    const int ProcessPowerThrottling = 4;
    const uint ExecutionSpeed = 1;
    static readonly object applyLock = new();
    static readonly string? currentExecutable = Path.GetFileNameWithoutExtension(Environment.ProcessPath);
    static readonly HashSet<string> protectedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Idle", "System", "Registry", "Secure System", "Memory Compression",
        "csrss", "wininit", "services", "lsass", "lsaiso", "smss", "winlogon", "dwm",
        "fontdrvhost", "svchost", "WattToolkit", "SteamTools", "Steam++",
        "BD.WTTS.Client.Avalonia.App", "BD.WTTS.Client.AppHost",
    };

    internal static bool IsProtectedName(int processId, string processName)
    {
        var name = Path.GetFileNameWithoutExtension(processName);
        return processId is 0 or 4 || processId == Environment.ProcessId ||
            protectedNames.Contains(processName) || protectedNames.Contains(name) ||
            string.Equals(processName, currentExecutable, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, currentExecutable, StringComparison.OrdinalIgnoreCase);
    }

    internal static byte[] ReadTopology()
    {
        uint length = 0;
        const int RelationAll = 0xffff;
        if (GetLogicalProcessorInformationEx(RelationAll, null, ref length) != 0 ||
            Marshal.GetLastPInvokeError() != ErrorInsufficientBuffer)
            return [];

        while (length is > 0 and <= 16 * 1024 * 1024)
        {
            var data = new byte[length];
            fixed (byte* buffer = data)
            {
                if (GetLogicalProcessorInformationEx(RelationAll, buffer, ref length) != 0)
                    return length == data.Length ? data : data.AsSpan(0, (int)length).ToArray();
                if (Marshal.GetLastPInvokeError() != ErrorInsufficientBuffer || length <= data.Length)
                    break;
            }
        }
        return [];
    }

    internal static CpuProcessState ReadProcess(int processId)
    {
        if (processId <= 0)
            return new(0, 0, null, null, null, null, null, true, false);

        using var process = OpenProcess(ProcessQueryLimitedInformation, 0, processId);
        if (process.IsInvalid)
            return new(0, 0, null, null, null, null, null, false,
                Marshal.GetLastPInvokeError() == CpuSchedulingResult.AccessDenied);
        if (GetProcessTimes(process, out long created, out _, out long kernel, out long user) == 0)
            return default;

        string? path = ReadImagePath(process);
        int priority = GetPriorityClass(process);
        uint memory = 0;
        PowerThrottling power = new() { Version = 1 };
        ulong? affinity = null;
        if (GetActiveProcessorGroupCount() == 1)
        {
            using var affinityProcess = OpenProcess(ProcessQueryInformation, 0, processId);
            if (!affinityProcess.IsInvalid &&
                GetProcessTimes(affinityProcess, out long affinityCreated, out _, out _, out _) != 0 && created == affinityCreated &&
                GetProcessAffinityMask(affinityProcess, out nuint mask, out _) != 0 && mask != 0)
                affinity = mask;
        }
        bool protectedProcess = (path is not null && IsProtectedName(processId, path)) ||
            IsProcessCritical(process, out int critical) == 0 || critical != 0 ||
            ProcessIdToSessionId(processId, out uint sessionId) == 0 || sessionId == 0;

        return new(created, kernel + user, path, priority == 0 ? null : (ProcessPriorityClass)priority,
            affinity,
            GetProcessInformation(process, ProcessPowerThrottling, &power, (uint)sizeof(PowerThrottling)) != 0 ? ReadEcoMode(power) : null,
            GetProcessInformation(process, ProcessMemoryPriority, &memory, sizeof(uint)) != 0 ? (int)memory : null,
            protectedProcess, false);
    }

    static CpuEcoMode ReadEcoMode(PowerThrottling state) => (state.ControlMask & ExecutionSpeed) == 0
        ? CpuEcoMode.SystemManaged
        : (state.StateMask & ExecutionSpeed) == 0 ? CpuEcoMode.Off : CpuEcoMode.On;

    internal static int Apply(int processId, long creationTime, int priority, ulong affinityMask,
        int ecoMode, int memoryPriority, bool restoreDefaults)
    {
        lock (applyLock)
            return ApplyCore(processId, creationTime, priority, affinityMask, ecoMode, memoryPriority, restoreDefaults);
    }

    static int ApplyCore(int processId, long creationTime, int priority, ulong affinityMask,
        int ecoMode, int memoryPriority, bool restoreDefaults)
    {
        if (processId <= 4 || processId == Environment.ProcessId)
            return CpuSchedulingResult.ProtectedProcess;
        if (creationTime <= 0 || (priority != 0 && !Enum.IsDefined((ProcessPriorityClass)priority)) ||
            ecoMode is < -1 or > (int)CpuEcoMode.On || memoryPriority is < 0 or > 5)
            return CpuSchedulingResult.InvalidSettings;

        bool singleGroup = GetActiveProcessorGroupCount() == 1;
        if (affinityMask != 0 && !singleGroup)
            return CpuSchedulingResult.AffinityUnavailable;

        bool changeAffinity = affinityMask != 0 || (restoreDefaults && singleGroup);
        uint rights = ProcessSetInformation | (changeAffinity ? ProcessQueryInformation : ProcessQueryLimitedInformation);
        using var process = OpenProcess(rights, 0, processId);
        if (process.IsInvalid)
            return Marshal.GetLastPInvokeError();
        if (GetProcessTimes(process, out long actualCreationTime, out _, out _, out _) == 0)
            return Marshal.GetLastPInvokeError();
        if (creationTime != actualCreationTime)
            return CpuSchedulingResult.ProcessChanged;
        var imagePath = ReadImagePath(process);
        if (imagePath is null || IsProtectedName(processId, imagePath) ||
            IsProcessCritical(process, out int critical) == 0 || critical != 0 ||
            ProcessIdToSessionId(processId, out uint sessionId) == 0 || sessionId == 0)
            return CpuSchedulingResult.ProtectedProcess;

        if (restoreDefaults)
        {
            priority = (int)ProcessPriorityClass.Normal;
            ecoMode = (int)CpuEcoMode.SystemManaged;
            memoryPriority = 5;
        }

        nuint oldAffinity = 0;
        nuint requestedAffinity = 0;
        if (changeAffinity)
        {
            if (GetProcessAffinityMask(process, out oldAffinity, out nuint systemAffinity) == 0)
                return Marshal.GetLastPInvokeError();
            if (oldAffinity == 0 || systemAffinity == 0 ||
                (!restoreDefaults && (affinityMask & (ulong)systemAffinity) != affinityMask))
                return CpuSchedulingResult.AffinityUnavailable;
            requestedAffinity = restoreDefaults ? systemAffinity : (nuint)affinityMask;
        }

        uint oldMemory = 0;
        if (memoryPriority != 0 && GetProcessInformation(process, ProcessMemoryPriority, &oldMemory, sizeof(uint)) == 0)
            return Marshal.GetLastPInvokeError();
        PowerThrottling oldPower = new() { Version = 1 };
        if (ecoMode >= 0 && GetProcessInformation(process, ProcessPowerThrottling, &oldPower, (uint)sizeof(PowerThrottling)) == 0)
        {
            int error = Marshal.GetLastPInvokeError();
            // Systems that do not implement execution-speed control can still reset other fields.
            if (!restoreDefaults || error is not (50 or 87))
                return error;
            ecoMode = -1;
        }

        bool didAffinity = changeAffinity && oldAffinity != requestedAffinity;
        if (didAffinity && SetProcessAffinityMask(process, requestedAffinity) == 0)
            return Marshal.GetLastPInvokeError();

        uint requestedMemory = (uint)memoryPriority;
        bool didMemory = requestedMemory != 0 && oldMemory != requestedMemory;
        int result = 0;
        if (didMemory && SetProcessInformation(process, ProcessMemoryPriority, &requestedMemory, sizeof(uint)) == 0)
        {
            result = Marshal.GetLastPInvokeError();
            didMemory = false;
        }

        bool didPower = false;
        if (result == 0 && ecoMode >= 0 && ReadEcoMode(oldPower) != (CpuEcoMode)ecoMode)
        {
            var power = oldPower;
            power.ControlMask = (power.ControlMask & ~ExecutionSpeed) |
                (ecoMode == (int)CpuEcoMode.SystemManaged ? 0u : ExecutionSpeed);
            power.StateMask = (power.StateMask & power.ControlMask & ~ExecutionSpeed) |
                (ecoMode == (int)CpuEcoMode.On ? ExecutionSpeed : 0u);
            didPower = SetProcessInformation(process, ProcessPowerThrottling, &power, (uint)sizeof(PowerThrottling)) != 0;
            if (!didPower)
                result = Marshal.GetLastPInvokeError();
        }

        // Priority comes last so a denied priority change cannot leave a realtime process behind.
        if (result == 0 && priority != 0 && SetPriorityClass(process, priority) == 0)
            result = Marshal.GetLastPInvokeError();
        if (result == 0)
            return CpuSchedulingResult.Success;

        if (didPower && SetProcessInformation(process, ProcessPowerThrottling, &oldPower, (uint)sizeof(PowerThrottling)) == 0)
            result = CpuSchedulingResult.RollbackFailed;
        if (didMemory && SetProcessInformation(process, ProcessMemoryPriority, &oldMemory, sizeof(uint)) == 0)
            result = CpuSchedulingResult.RollbackFailed;
        if (didAffinity && SetProcessAffinityMask(process, oldAffinity) == 0)
            result = CpuSchedulingResult.RollbackFailed;
        return result;
    }

    static string? ReadImagePath(SafeProcessHandle process)
    {
        Span<char> path = stackalloc char[512];
        uint chars = (uint)path.Length;
        fixed (char* buffer = path)
        {
            if (QueryFullProcessImageName(process, 0, buffer, ref chars) != 0)
                return new(buffer, 0, (int)chars);
            if (Marshal.GetLastPInvokeError() != ErrorInsufficientBuffer)
                return null;
        }
        path = new char[32768];
        chars = (uint)path.Length;
        fixed (char* buffer = path)
            return QueryFullProcessImageName(process, 0, buffer, ref chars) != 0 ? new(buffer, 0, (int)chars) : null;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PowerThrottling
    {
        public uint Version;
        public uint ControlMask;
        public uint StateMask;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int GetLogicalProcessorInformationEx(int relationship, byte* buffer, ref uint length);

    [LibraryImport("kernel32.dll")]
    private static partial ushort GetActiveProcessorGroupCount();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial SafeProcessHandle OpenProcess(uint access, int inheritHandle, int processId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int GetProcessTimes(SafeProcessHandle process, out long creation, out long exit, out long kernel, out long user);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int IsProcessCritical(SafeProcessHandle process, out int critical);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int ProcessIdToSessionId(int processId, out uint sessionId);

    [LibraryImport("kernel32.dll", EntryPoint = "QueryFullProcessImageNameW", SetLastError = true)]
    private static partial int QueryFullProcessImageName(SafeProcessHandle process, uint flags, char* path, ref uint size);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int GetPriorityClass(SafeProcessHandle process);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int SetPriorityClass(SafeProcessHandle process, int priority);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int GetProcessAffinityMask(SafeProcessHandle process, out nuint processMask, out nuint systemMask);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int SetProcessAffinityMask(SafeProcessHandle process, nuint mask);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int GetProcessInformation(SafeProcessHandle process, int informationClass, void* information, uint size);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int SetProcessInformation(SafeProcessHandle process, int informationClass, void* information, uint size);
}
#endif
