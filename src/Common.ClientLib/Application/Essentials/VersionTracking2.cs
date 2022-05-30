using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using _ThisAssembly = System.Properties.ThisAssembly;
// https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/VersionTracking/VersionTracking.shared.cs
// https://github.com/dotnet/maui/blob/main/src/Essentials/src/VersionTracking/VersionTracking.shared.cs

// ReSharper disable once CheckNamespace
namespace System.Application;

/// <summary>
/// 提供一种在设备上跟踪应用程序版本的简便方法。
/// </summary>
public static class VersionTracking2
{
    const string versionsKey = "VersionTracking.Versions";
    const string sharedName = "steam++.xamarinessentials.versiontracking";

    static List<string>? versionTrail;

    static VersionTracking2()
    {
        InitVersionTracking();
    }

    /// <summary>
    /// Initialize VersionTracking module, load data and track current version
    /// </summary>
    /// <remarks>
    /// For internal use. Usually only called once in production code, but multiple times in unit tests
    /// </remarks>
    internal static void InitVersionTracking()
    {
        IsFirstLaunchEver = !Preferences2.ContainsKey(versionsKey, sharedName);
        if (IsFirstLaunchEver)
        {
            versionTrail = new();
        }
        else
        {
            versionTrail = ReadHistory(versionsKey).ToList();
        }

        IsFirstLaunchForCurrentVersion = !versionTrail.Contains(CurrentVersion) || LastInstalledVersion() != CurrentVersion;
        if (IsFirstLaunchForCurrentVersion)
        {
            versionTrail.Add(CurrentVersion);
        }

        if (IsFirstLaunchForCurrentVersion)
        {
            WriteHistory(versionsKey, versionTrail);
        }
    }

    /// <summary>
    /// 获取应用程序的当前版本号。
    /// </summary>
    public const string CurrentVersion = _ThisAssembly.Version;

    /// <summary>
    /// 获取此设备上安装的应用程序的第一个版本的版本号。
    /// </summary>
    public static string FirstInstalledVersion => versionTrail!.FirstOrDefault();

    static bool mIsFirstLaunchEver;

    /// <summary>
    /// 获取一个值，该值指示此应用是否首次在此设备上启动。
    /// </summary>
    public static bool IsFirstLaunchEver
    {
        get => mIsFirstLaunchEver;

        private set => mIsFirstLaunchEver = value;
    }

    static bool mIsFirstLaunchForCurrentVersion;

    /// <summary>
    /// 获取一个值，该值指示这是否是当前版本号的应用程序的首次启动。
    /// </summary>
    public static bool IsFirstLaunchForCurrentVersion
    {
        get => mIsFirstLaunchForCurrentVersion;

        private set => mIsFirstLaunchForCurrentVersion = value;
    }

    /// <summary>
    /// 获取以前运行的版本的版本号。
    /// </summary>
    public static string? PreviousVersion => GetPrevious(versionTrail!);

    /// <summary>
    /// 获取在此设备上运行的应用的版本号集合。
    /// </summary>
    public static IEnumerable<string> VersionHistory => versionTrail!.ToArray();

    /// <summary>
    /// 确定这是否是指定版本号的应用程序的首次启动。
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool IsFirstLaunchForVersion(string version)
        => version == CurrentVersion && IsFirstLaunchForCurrentVersion;

    /// <summary>
    /// 开始跟踪版本信息。
    /// </summary>
#if NETSTANDARD
    [Preserve]
#endif
    public static void Track()
    {
        // 调用空函数触发当前类静态构造函数
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string[] ReadHistory(string key)
        => Preferences2.Get(key, null, sharedName)?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void WriteHistory(string key, IEnumerable<string> history)
        => Preferences2.Set(key, string.Join("|", history), sharedName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string? GetPrevious(List<string> trail)
    {
        return (trail.Count >= 2) ? trail[^2] : null;
    }

    static string LastInstalledVersion() => versionTrail.LastOrDefault();
}
