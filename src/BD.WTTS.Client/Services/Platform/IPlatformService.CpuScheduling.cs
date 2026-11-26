namespace BD.WTTS.Services;

public readonly record struct CpuProcessState(
    long CreationTime,
    long CpuTime,
    string? ImagePath,
    ProcessPriorityClass? Priority,
    ulong? AffinityMask,
    CpuEcoMode? EcoMode,
    int? MemoryPriority,
    bool IsProtected,
    bool AccessDenied);

public enum CpuEcoMode
{
    SystemManaged,
    Off,
    On,
}

public static class CpuSchedulingResult
{
    public const int Success = 0;
    public const int AccessDenied = 5;
    public const int ProtectedProcess = -1;
    public const int ProcessChanged = -2;
    public const int AffinityUnavailable = -3;
    public const int InvalidSettings = -4;
    public const int RollbackFailed = -5;
    public const int Unsupported = -6;
}

partial interface IPlatformService
{
    byte[] GetCpuSchedulingTopology() => [];

    CpuProcessState GetCpuSchedulingProcessState(int processId) => default;

    bool IsCpuSchedulingProtectedProcess(int processId, string processName) => true;

    Task<int> ApplyCpuSchedulingAsync(int processId, long creationTime, int priority, ulong affinityMask,
        int ecoMode, int memoryPriority, bool restoreDefaults) => Task.FromResult(CpuSchedulingResult.Unsupported);
}
