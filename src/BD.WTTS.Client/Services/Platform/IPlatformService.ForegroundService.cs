// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 是否使用平台前台服务
    /// </summary>
    [Mobius(Obsolete = true)]
    bool UsePlatformForegroundService => false;

    /// <summary>
    /// 启动或停止前台服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="startOrStop">启动或停止或取反</param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    [Mobius(Obsolete = true)]
    Task StartOrStopForegroundServiceAsync(string serviceName, bool? startOrStop = null)
        => throw new PlatformNotSupportedException();
}