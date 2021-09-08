using Xamarin.Essentials;
using XEDeviceIdiom = Xamarin.Essentials.DeviceIdiom;

namespace System.Application
{
    public static class DeviceInfo2
    {
        public static Platform Platform
        {
            get
            {
                if (OperatingSystem2.IsWindows)
                {
                    if (DeviceInfo.Platform == DevicePlatform.UWP)
                    {
                        return Platform.UWP;
                    }
                    else
                    {
                        return Platform.Windows;
                    }
                }
                else if (OperatingSystem2.IsAndroid)
                {
                    return Platform.Android;
                }
                else if (
                    OperatingSystem2.IsIOS ||
                    OperatingSystem2.IsMacOS ||
                    OperatingSystem2.IsTvOS ||
                    OperatingSystem2.IsWatchOS)
                {
                    return Platform.Apple;
                }
                else if (OperatingSystem2.IsLinux)
                {
                    return Platform.Linux;
                }
                return Platform.Unknown;
            }
        }

        public static DeviceIdiom Idiom
        {
            get
            {
                if (OperatingSystem2.IsDesktop)
                {
                    return DeviceIdiom.Desktop;
                }
                else
                {
                    return DeviceInfo.Idiom.Convert();
                }
            }
        }

        public static string? OSName
        {
            get
            {
                if (OperatingSystem2.IsWindows)
                {
                    if (DesktopBridge.IsRunningAsUwp)
                    {
                        return "Windows Desktop Bridge";
                    }
                    else if (DeviceInfo.Platform == DevicePlatform.UWP)
                    {
                        return "UWP";
                    }
                    else
                    {
                        return "Windows";
                    }
                }
                else if (OperatingSystem2.IsAndroid)
                {
                    return "Android";
                }
                else if (OperatingSystem2.IsIOS)
                {
                    if (DeviceInfo.Idiom == XEDeviceIdiom.Tablet)
                        return "iPadOS";
                    return "iOS";
                }
                else if (OperatingSystem2.IsMacOS)
                {
                    return "macOS";
                }
                else if (OperatingSystem2.IsTvOS)
                {
                    return "tvOS";
                }
                else if (OperatingSystem2.IsWatchOS)
                {
                    return "watchOS";
                }
                else if (OperatingSystem2.IsLinux)
                {
                    return "Linux";
                }
                return default;
            }
        }
    }
}