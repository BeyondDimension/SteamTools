using System.Application.Services;
using System.Runtime.CompilerServices;
using EDeviceType = System.DeviceType;
using EPlatform = System.Platform;

// ReSharper disable once CheckNamespace
namespace System.Application;

public static class DeviceInfo2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Model() => IDeviceInfoPlatformService.Interface?.Model ?? string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Manufacturer() => IDeviceInfoPlatformService.Interface?.Manufacturer ?? string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Name() => IDeviceInfoPlatformService.Interface?.Name ?? string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string VersionString() => IDeviceInfoPlatformService.Interface?.VersionString ?? string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EPlatform Platform() => IDeviceInfoPlatformService.Platform;

    public static DeviceIdiom Idiom()
    {
        var i = IDeviceInfoPlatformService.Interface;
        if (i != null)
        {
            var value = i.Idiom;
            if (value != DeviceIdiom.Unknown) return value;
        }
        if (OperatingSystem2.IsDesktop())
        {
            return DeviceIdiom.Desktop;
        }
        return DeviceIdiom.Unknown;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string OSName() => OSNameValue().ToDisplayName();

    static readonly Lazy<OSNames.Value> _OSNameValue = new(() =>
    {
        if (OperatingSystem2.IsWindows())
        {
            var i = IDeviceInfoPlatformService.Interface;
            if (i != null && i.IsUWP)
            {
                return OSNames.Value.UWP;
            }
            else if (DesktopBridge.IsRunningAsUwp)
            {
                return OSNames.Value.WindowsDesktopBridge;
            }
            else
            {
                return OSNames.Value.Windows;
            }
        }
        else if (OperatingSystem2.IsAndroid())
        {
            if (OperatingSystem2.IsRunningOnWSA())
            {
                return OSNames.Value.WSA;
            }
            var i = IDeviceInfoPlatformService.Interface;
            if (i != null && i.IsChromeOS)
            {
                return OSNames.Value.ChromeOS;
            }
            else
            {
                if (DeviceType() == EDeviceType.Virtual)
                {
                    return OSNames.Value.AndroidVirtual;
                }
                return Idiom() switch
                {
                    DeviceIdiom.Phone => OSNames.Value.AndroidPhone,
                    DeviceIdiom.Tablet => OSNames.Value.AndroidTablet,
                    DeviceIdiom.Desktop => OSNames.Value.AndroidDesktop,
                    DeviceIdiom.TV => OSNames.Value.AndroidTV,
                    DeviceIdiom.Watch => OSNames.Value.AndroidWatch,
                    _ => OSNames.Value.AndroidUnknown,
                };
            }
        }
        else if (OperatingSystem2.IsIOS())
        {
            if (Idiom() == DeviceIdiom.Tablet)
                return OSNames.Value.iPadOS;
            return OSNames.Value.iOS;
        }
        else if (OperatingSystem2.IsMacOS())
        {
            return OSNames.Value.macOS;
        }
        else if (OperatingSystem2.IsTvOS())
        {
            return OSNames.Value.tvOS;
        }
        else if (OperatingSystem2.IsWatchOS())
        {
            return OSNames.Value.watchOS;
        }
        else if (OperatingSystem2.IsLinux())
        {
            return OSNames.Value.Linux;
        }
        return default;
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OSNames.Value OSNameValue() => _OSNameValue.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DeviceType DeviceType()
    {
        var i = IDeviceInfoPlatformService.Interface;
        if (i != null) return i.DeviceType;
        return default;
    }
}