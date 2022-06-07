namespace System.Application.Services.Implementation;

public class DeviceInfoPlatformServiceImpl : IDeviceInfoPlatformService
{
    public virtual string Model => DeviceInfo.Model;

    public virtual string Manufacturer => DeviceInfo.Manufacturer;

    public virtual string Name => DeviceInfo.Name;

    public virtual string VersionString => DeviceInfo.VersionString;

    public virtual DeviceType DeviceType => DeviceInfo.DeviceType.Convert();

    public virtual bool IsChromeOS => false;

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable CS0618 // 类型或成员已过时
    public virtual bool IsUWP => DeviceInfo.Platform == DevicePlatform.UWP;
#pragma warning restore CS0618 // 类型或成员已过时
#pragma warning restore IDE0079 // 请删除不必要的忽略

    public virtual bool IsWinUI =>
#if MAUI
        DeviceInfo.Platform == DevicePlatform.WinUI;
#else
        false;
#endif

    public virtual DeviceIdiom Idiom => DeviceInfo.Idiom.Convert();
}
