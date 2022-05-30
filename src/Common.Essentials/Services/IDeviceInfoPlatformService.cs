using EPlatform = System.Platform;

namespace System.Application.Services;

public interface IDeviceInfoPlatformService
{
    static IDeviceInfoPlatformService? Interface => DI.Get_Nullable<IDeviceInfoPlatformService>();

    string Model { get; }

    string Manufacturer { get; }

    string Name { get; }

    string VersionString { get; }

    DeviceType DeviceType { get; }

    bool IsChromeOS { get; }

    bool IsUWP { get; }

    DeviceIdiom Idiom { get; }

    static EPlatform Platform
    {
        get
        {
            if (OperatingSystem2.IsWindows())
            {
                var i = Interface;
                if (i != null && i.IsUWP)
                {
                    return EPlatform.UWP;
                }
                else
                {
                    return EPlatform.Windows;
                }
            }
            else if (OperatingSystem2.IsAndroid())
            {
                return EPlatform.Android;
            }
            else if (
                OperatingSystem2.IsIOS() ||
                OperatingSystem2.IsMacOS() ||
                OperatingSystem2.IsTvOS() ||
                OperatingSystem2.IsWatchOS())
            {
                return EPlatform.Apple;
            }
            else if (OperatingSystem2.IsLinux())
            {
                return EPlatform.Linux;
            }
            return EPlatform.Unknown;
        }
    }
}