namespace System.Application.Markup
{
    public sealed class OnPlatformExtension : MarkupExtension<bool>
    {
        public OnPlatformExtension(string member) : base(member)
        {

        }

        public override bool ProvideValue(IServiceProvider serviceProvider) =>
            member switch
            {
                "Windows" => OperatingSystem2.IsWindows(),
                "Windows7" => OperatingSystem2.IsWindows7(),
                "Windows10AtLeast" => OperatingSystem2.IsWindows10AtLeast(),
                "Windows11AtLeast" => OperatingSystem2.IsWindows11AtLeast(),
                "macOS" => OperatingSystem2.IsMacOS(),
                "iOS" => OperatingSystem2.IsIOS(),
                "iPhone" => OperatingSystem2.IsIOS() && DeviceInfo2.Idiom() == DeviceIdiom.Phone,
                "iPadOS" => OperatingSystem2.IsIOS() && DeviceInfo2.Idiom() == DeviceIdiom.Tablet,
                "watchOS" => OperatingSystem2.IsWatchOS(),
                "tvOS" => OperatingSystem2.IsTvOS(),
                "Linux" => OperatingSystem2.IsLinux(),
                "LinuxDesktop" => OperatingSystem2.IsLinux() && !OperatingSystem2.IsAndroid(),
                "Android" => OperatingSystem2.IsAndroid(),
                "AndroidDesktop" => OperatingSystem2.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.Desktop,
                "AndroidPhone" => OperatingSystem2.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.Phone,
                "AndroidTablet" => OperatingSystem2.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.Tablet,
                "AndroidWatch" => OperatingSystem2.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.Watch,
                "AndroidTV" => OperatingSystem2.IsAndroid() && DeviceInfo2.Idiom() == DeviceIdiom.TV,
                "WSA" => OperatingSystem2.IsRunningOnWSA(),
                "Mono" => OperatingSystem2.IsRunningOnMono(),
                "Windows10PackageIdentity" => OperatingSystem2.IsRunningAsUwp(),
                "DesktopBridge" => DesktopBridge.IsRunningAsUwp,
                _ => false,
            };
    }
}
