using Android.App;
using Android.Content;
using System.Linq;
using Process = Android.OS.Process;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ActivityManagerExtensions
    {
        /// <summary>
        /// 获取当前进程名称
        /// </summary>
        /// <param name="activityManager"></param>
        /// <returns></returns>
        public static string? GetCurrentProcessName(this ActivityManager activityManager)
            => activityManager.RunningAppProcesses?.
            FirstOrDefault(x => x != null && x.Pid == Process.MyPid())?.ProcessName;

        /// <inheritdoc cref="GetCurrentProcessName(ActivityManager)"/>
        public static string? GetCurrentProcessName(this Context context)
            => context.GetActivityManager().GetCurrentProcessName();

        public static ActivityManager GetActivityManager(this Context context)
            => context.GetSystemService<ActivityManager>();
    }
}