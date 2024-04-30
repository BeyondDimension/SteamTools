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
#if WINDOWS || MACOS || LINUX
        => true;
#elif MACCATALYST
        => OperatingSystem.IsMacOS();
#elif IOS || ANDROID
        => false;
#else
        => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid());
#endif

    [Obsolete("use IsDesktop", true)]
    static bool IsDesktopPlatform => IsDesktop();

    /// <summary>
    /// 获取当前平台 UI Host
    /// <para>reference to the ViewController (if using Xamarin.iOS), Activity (if using Xamarin.Android) IWin32Window or IntPtr (if using .Net Framework).</para>
    /// </summary>
    object CurrentPlatformUIHost { get; }

#if WINDOWS
    private static readonly Lazy<DeploymentMode> _DeploymentMode = new(() =>
    {
        try
        {
            var path = typeof(object).Assembly.Location;
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (path.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase))
            {
                return DeploymentMode.FDE;
            }
        }
        catch
        {
        }
        return DeploymentMode.SCD;
    });
#endif

    DeploymentMode DeploymentMode =>
#if WINDOWS
        _DeploymentMode.Value;
#else
        DeploymentMode.SCD;
#endif

    /// <inheritdoc cref="IPlatformService.SetBootAutoStart(bool, string)"/>
    static void SetBootAutoStart(bool isAutoStart) => IPlatformService.Instance.SetBootAutoStart(isAutoStart, Constants.HARDCODED_APP_NAME);

    private static readonly Lazy<ClientPlatform> clientPlatform = new(() =>
    {
        var platform = DeviceInfo2.Platform();
        //var deviceIdiom = DeviceInfo2.Idiom();
        switch (platform)
        {
#if WINDOWS
#pragma warning disable CS0612 // 类型或成员已过时
            case Platform.Windows:
            case Platform.WinUI:
            case Platform.UWP:
#pragma warning restore CS0612 // 类型或成员已过时
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X86:
                        return DesktopBridge.IsRunningAsUwp ?
                        ClientPlatform.Win32StoreX86 : ClientPlatform.Win32X86;
                    case Architecture.X64:
                        return DesktopBridge.IsRunningAsUwp ?
                        ClientPlatform.Win32StoreX64 : ClientPlatform.Win32X64;
                    case Architecture.Arm64:
                        return DesktopBridge.IsRunningAsUwp ?
                        ClientPlatform.Win32StoreArm64 : ClientPlatform.Win32Arm64;
                }
                break;
#elif ANDROID
            case Platform.Android:
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X86:
                        return ClientPlatform.AndroidPadX86;
                    case Architecture.X64:
                        return ClientPlatform.AndroidPhoneX64;
                    case Architecture.Arm64:
                        return ClientPlatform.AndroidPhoneArm64;
                    case Architecture.Arm:
                        return ClientPlatform.AndroidPhoneArm;
                }
                break;
#elif MACCATALYST || MACOS || IOS
            case Platform.Apple:
                switch (DeviceInfo2.Idiom())
                {
#if !IOS
                    case DeviceIdiom.Desktop:
                        switch (RuntimeInformation.ProcessArchitecture)
                        {
                            case Architecture.X64:
                                return ClientPlatform.macOSX64;
                            case Architecture.Arm64:
                                return ClientPlatform.macOSArm64;
                        }
                        break;
#endif
#if !MACOS
                    case DeviceIdiom.Phone:
                        return ClientPlatform.iOSArm64;
                    case DeviceIdiom.Tablet:
                        return ClientPlatform.iPadOSArm64;
                    case DeviceIdiom.TV:
                        return ClientPlatform.tvOSArm64;
                    case DeviceIdiom.Watch:
                        return ClientPlatform.watchOSArm64;
#endif
                }
                break;
#else
            case Platform.Linux:
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X64:
                        return ClientPlatform.LinuxX64;
                    case Architecture.Arm64:
                        return ClientPlatform.LinuxArm64;
                    case Architecture.Arm:
                        return ClientPlatform.LinuxArm;
                }
                break;
#endif
        }
        return default;
    });

    static byte[] Login_512 => Properties.Resources.AppLogo_512;

    static ClientPlatform ClientPlatform => clientPlatform.Value;

    System.Drawing.Size? GetScreenSize() => null;

    bool? IsAnyWindowNotMinimized() => null;

    async void OpenBrowserCommandCore(object? url) => await Browser2.OpenAsync(url?.ToString());
}
