// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// 当前应用程序，可能是 UI 程序，也可能是控制台程序
/// </summary>
public partial interface IApplication
{
    static IApplication Instance => Ioc.Get<IApplication>();

    /// <summary>
    /// 当前是否为桌面端，IsWindows or IsMacOS or (IsLinux and !IsAndroid)
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsDesktop()
        => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid());

    [Obsolete("use IsDesktop", true)]
    static bool IsDesktopPlatform => IsDesktop();

    /// <summary>
    /// 获取当前平台 UI Host
    /// <para>reference to the ViewController (if using Xamarin.iOS), Activity (if using Xamarin.Android) IWin32Window or IntPtr (if using .Net Framework).</para>
    /// </summary>
    object CurrentPlatformUIHost { get; }
}
