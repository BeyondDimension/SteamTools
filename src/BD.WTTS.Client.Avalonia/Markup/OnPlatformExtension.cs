using BD.WTTS.Markup.Abstractions;

namespace BD.WTTS.Markup;

public sealed class OnPlatformExtension : MarkupExtension<bool>
{
    public OnPlatformExtension(string member) : base(member)
    {

    }

    public sealed override bool ProvideValue(IServiceProvider serviceProvider) =>
        member switch
        {
            "Windows" => OperatingSystem.IsWindows(),
            //"Windows7" => OperatingSystem2.IsWindows7(),
            "Windows10AtLeast" => OperatingSystem2.IsWindows10AtLeast(),
            "Windows11AtLeast" => OperatingSystem2.IsWindows11AtLeast(),
            "macOS" => OperatingSystem.IsMacOS(),
            "iOS" => OperatingSystem.IsIOS(),
            "iPhone" => OperatingSystem.IsIOS() && DeviceInfo2.Idiom() == DeviceIdiom.Phone,
            "iPadOS" => OperatingSystem.IsIOS() && DeviceInfo2.Idiom() == DeviceIdiom.Tablet,
            "watchOS" => OperatingSystem.IsWatchOS(),
            "tvOS" => OperatingSystem.IsTvOS(),
            "Linux" => OperatingSystem.IsLinux(),
            "LinuxDesktop" => OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid(),
            "Android" => OperatingSystem.IsAndroid(),
            "AndroidDesktop" => OperatingSystem.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.Desktop,
            "AndroidPhone" => OperatingSystem.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.Phone,
            "AndroidTablet" => OperatingSystem.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.Tablet,
            "AndroidWatch" => OperatingSystem.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.Watch,
            "AndroidTV" => OperatingSystem.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.TV,
            "WSA" => OperatingSystem2.IsRunningOnWSA(),
            "Mono" => OperatingSystem2.IsRunningOnMono(),
            "Windows10PackageIdentity" => OperatingSystem2.IsRunningAsUwp(),
            "DesktopBridge" => DesktopBridge.IsRunningAsUwp,
            "SteamApp" => Startup.Instance.IsSteamRun,
            "OfficialRelease" => !(Startup.Instance.IsSteamRun || DesktopBridge.IsRunningAsUwp),
            "Preview" => AssemblyInfo.IsPreview,
            "RC" => AssemblyInfo.IsReleaseCandidate,
            "GA" => AssemblyInfo.IsGeneralAvailability,
            "!GA" => !AssemblyInfo.IsGeneralAvailability,
            _ => false,
        };
}