namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    // Zero priority/memory/affinity and a negative EcoQoS mode leave those settings alone.
    Task<int> SetCpuSchedulingAsync(int processId, long creationTime, int priority, ulong affinityMask,
        int ecoMode, int memoryPriority, bool restoreDefaults) => Task.FromException<int>(new PlatformNotSupportedException());
}
