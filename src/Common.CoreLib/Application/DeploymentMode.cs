namespace System.Application
{
    /// <summary>
    /// 应用程序部署模式
    /// <para>https://docs.microsoft.com/zh-cn/dotnet/core/deploying</para>
    /// </summary>
    public enum DeploymentMode : byte
    {
        /// <summary>
        /// 将应用发布为独立应用，将生成特定于平台的可执行文件。 输出发布文件夹包含应用的所有组件，包括 .NET 库和目标运行时。 应用独立于其他 .NET 应用，且不使用本地安装的共享运行时。 应用的用户无需下载和安装 .NET。
        /// </summary>
        SCD,

        /// <summary>
        /// 如果将应用发布为依赖于框架的应用，则该应用是跨平台的，且不包含 .NET 运行时。 应用的用户需要安装 .NET 运行时。
        /// <para>如果将应用发布为依赖于框架的应用，会以 dll 文件的形式生成一个跨平台二进制文件，还会生成面向当前平台的特定于平台的可执行文件。 dll 是跨平台的，而可执行文件不是。 </para>
        /// </summary>
        FDE,
    }
}
