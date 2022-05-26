using System.Runtime.CompilerServices;
using Xamarin.Essentials;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Enum 扩展 <see cref="DevicePlatform"/>
/// </summary>
public static partial class DevicePlatformExtensions
{
    static readonly IReadOnlyDictionary<DevicePlatform, Platform> mapping = new Dictionary<DevicePlatform, Platform>
    {
        { DevicePlatform.Android, Platform.Android },
        { DevicePlatform.iOS, Platform.Apple },
        { DevicePlatform.UWP, Platform.UWP },
        { DevicePlatform.tvOS, Platform.Apple },
        { DevicePlatform.Tizen, Platform.Linux },
        { DevicePlatform.watchOS, Platform.Apple },
    };

    /// <summary>
    /// 将 <see cref="DevicePlatform"/> 转换为 <see cref="Platform"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Platform Convert(this DevicePlatform value) => mapping.ContainsKey(value) ? mapping[value] : Platform.Unknown;
}
