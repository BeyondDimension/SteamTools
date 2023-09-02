// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 获取 排除代理地址
    /// </summary>
    protected static string[] GetNoProxyHostName => new string[] {
        "10.*",
        "172.16.*",
        "172.17.*",
        "172.18.*",
        "172.19.*",
        "172.20.*",
        "172.21.*",
        "172.22.*",
        "172.23.*",
        "172.24.*",
        "172.25.*",
        "172.26.*",
        "172.27.*",
        "172.28.*",
        "172.29.*",
        "172.30.*",
        "172.31.*",
        "192.168.*",
        Constants.Urls.OfficialApiHostName,
        Constants.Urls.OfficialShopApiHostName,
    };

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