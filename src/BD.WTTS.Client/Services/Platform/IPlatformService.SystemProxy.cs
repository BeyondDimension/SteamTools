// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 设置启用或关闭系统代理
    /// </summary>
    bool SetAsSystemProxy(bool state, IPAddress? ip = null, int port = -1) => false;

    /// <summary>
    /// 设置启用或关闭 PAC 代理
    /// </summary>
    /// <param name="state"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    bool SetAsSystemPACProxy(bool state, string? url = null) => false;
}