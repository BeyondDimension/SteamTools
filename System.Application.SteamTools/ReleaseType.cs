namespace System.Application
{
    /// <summary>
    /// 发行版本类型
    /// <para>枚举常量命名参考：https://dotnet.microsoft.com/download/dotnet/5.0 </para>
    /// </summary>
    public enum ReleaseType
    {
        /// <summary>
        /// Windows 64 bit
        /// </summary>
        Windows_x64,

        /// <summary>
        /// Windows 32 bit
        /// </summary>
        Windows_x86,

        /// <summary>
        /// Windows平台下当用户不知道是否运行的64位系统时推荐使用此版本(不兼容<see cref="Windows_Arm64"/>)，可通过后续更新到 <see cref="Windows_x64"/> 或 <see cref="Windows_x86"/>
        /// </summary>
        Windows_Any,

        /// <summary>
        /// Windows ARM 64 bit
        /// <para>例如设备：Surface Pro X</para>
        /// </summary>
        Windows_Arm64,

        /// <summary>
        /// 使用 Intel 的 Mac 设备
        /// </summary>
        macOS_x64,

        /// <summary>
        /// 使用 Apple Silicon 的 Mac 设备
        /// </summary>
        macOS_Arm64,

        /// <summary>
        /// Linux 64 bit
        /// </summary>
        Linux_x64,

        /// <summary>
        /// Apple (iPhone / iPad / iPod touch)
        /// </summary>
        iOS,

        /// <summary>
        /// Android arm64-v8a
        /// <para>例如高通骁龙808，810等2015年后发布的Soc</para>
        /// </summary>
        Android_Arm64,

        /// <summary>
        /// Android armeabi-v7a
        /// <para>安卓平台下当用户不知道是否运行的64位系统时推荐使用此版本，可通过后续更新到 <see cref="Android_Arm64"/></para>
        /// </summary>
        Android_Arm32,
    }
}