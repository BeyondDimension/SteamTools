using System.Runtime.InteropServices;

namespace System;

/// <summary>
/// CPU 构架，例如 x86、Arm 等 <see cref="Architecture"/>
/// 例如不同的 Android 设备使用不同的 CPU，而不同的 CPU 支持不同的指令集。
/// <para>CPU 与指令集的每种组合都有专属的应用二进制接口 (ABI)。</para>
/// <para>https://developer.android.google.cn/ndk/guides/abis.html</para>
/// <para>在比较版本支持的值与设备支持的值中的交集中，值越大的，优先选取</para>
/// </summary>
[Flags]
public enum ArchitectureFlags
{
    /// <summary>
    /// https://developer.android.google.cn/ndk/guides/abis.html#x86
    /// </summary>
    X86 = 16,

    /// <summary>
    /// https://developer.android.google.cn/ndk/guides/abis.html#v7a
    /// </summary>
    Arm = 32,

    /// <summary>
    /// https://developer.android.google.cn/ndk/guides/abis.html#86-64
    /// </summary>
    X64 = 128,

    /// <summary>
    /// https://developer.android.google.cn/ndk/guides/abis.html#arm64-v8a
    /// </summary>
    Arm64 = 256,
}