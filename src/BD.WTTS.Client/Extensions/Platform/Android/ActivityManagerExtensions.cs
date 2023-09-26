#if ANDROID

// ReSharper disable once CheckNamespace
namespace Android;

public static class ActivityManagerExtensions
{
    /// <summary>
    /// 获取当前进程名称
    /// </summary>
    /// <param name="activityManager"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetCurrentProcessName(this ActivityManager activityManager)
        => activityManager.RunningAppProcesses?.
        FirstOrDefault(x => x != null && x.Pid == AndroidProcess.MyPid())?.ProcessName;

    /// <inheritdoc cref="GetCurrentProcessName(ActivityManager)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetCurrentProcessName(this Context context)
        => context.GetActivityManager().GetCurrentProcessName();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActivityManager GetActivityManager(this Context context)
        => context.GetSystemService<ActivityManager>();
}
#endif