using XEDeviceIdiom = Xamarin.Essentials.DeviceIdiom;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Enum 扩展 <see cref="XEDeviceIdiom"/>
/// </summary>
public static partial class DeviceIdiomExtensions
{
    static readonly IReadOnlyDictionary<XEDeviceIdiom, DeviceIdiom> mapping = new Dictionary<XEDeviceIdiom, DeviceIdiom>
    {
        { XEDeviceIdiom.Phone, DeviceIdiom.Phone },
        { XEDeviceIdiom.Tablet, DeviceIdiom.Tablet },
        { XEDeviceIdiom.Desktop, DeviceIdiom.Desktop },
        { XEDeviceIdiom.TV, DeviceIdiom.TV },
        { XEDeviceIdiom.Watch, DeviceIdiom.Watch },
    };

    /// <summary>
    /// 将 <see cref="Xamarin.Essentials.DeviceIdiom"/> 转换为 <see cref="DeviceIdiom"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static DeviceIdiom Convert(this XEDeviceIdiom value) => mapping.ContainsKey(value) ? mapping[value] : DeviceIdiom.Unknown;
}
