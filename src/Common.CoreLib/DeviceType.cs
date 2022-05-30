namespace System;

/// <summary>
/// 设备类型。
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// 一个未知的设备类型。
    /// </summary>
    Unknown,

    /// <summary>
    /// 该设备是一个物理设备，如 iPhone、Android 平板电脑或 Windows 桌面。
    /// </summary>
    Physical,

    /// <summary>
    /// 该设备是虚拟的，如 iOS 模拟器、Android 模拟器或 Windows 模拟器。
    /// </summary>
    Virtual,
}