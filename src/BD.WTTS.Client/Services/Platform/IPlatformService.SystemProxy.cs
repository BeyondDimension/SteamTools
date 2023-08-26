// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 设置启用或关闭系统代理
    /// </summary>
    Task<bool> SetAsSystemProxyAsync(bool state, IPAddress? ip = null, int port = -1) => Task.FromResult(false);

    /// <summary>
    /// 设置启用或关闭 PAC 代理
    /// </summary>
    /// <param name="state"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<bool> SetAsSystemPACProxyAsync(bool state, string? url = null) => Task.FromResult(false);
}